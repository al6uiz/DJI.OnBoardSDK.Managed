namespace DJI.OnBoardSDK
{
    partial class CoreAPI
	{

		void setupMMU()
		{
			for (int i = 0; i < MMU.Length; i++)
			{
				MMU[i] = new MMU_Tab();
			}

			MMU[0].tabIndex = 0;
			MMU[0].usageFlag = true;
			MMU[0].pmem = (Ptr)memory;
			MMU[0].memSize = 0;
			for (var i = 1; i < (MMU_TABLE_NUM - 1); i++)
			{
				MMU[i].tabIndex = i;
				MMU[i].usageFlag = false;
			}
			MMU[MMU_TABLE_NUM - 1].tabIndex = MMU_TABLE_NUM - 1;
			MMU[MMU_TABLE_NUM - 1].usageFlag = true;
			MMU[MMU_TABLE_NUM - 1].pmem = (Ptr)memory + MEMORY_SIZE;
			MMU[MMU_TABLE_NUM - 1].memSize = 0;
		}

		void freeMemory(MMU_Tab mmu_tab)
		{
			if (mmu_tab == null)
				return;
			if (mmu_tab.tabIndex == 0 || mmu_tab.tabIndex == (MMU_TABLE_NUM - 1))
				return;
			mmu_tab.usageFlag = false;
		}

			int[] mmu_tab_used_index = new int[MMU_TABLE_NUM];

		MMU_Tab allocMemory(int size)
		{
			int mem_used = 0;
			byte i;
			byte j = 0;
			byte mmu_tab_used_num = 0;
			int temp32;
			var temp_area = new int[2] { int.MaxValue, int.MaxValue };

			int record_temp32 = 0;
			byte magic_flag = 0;

			if (size > PRO_PURE_DATA_MAX_SIZE || size > MEMORY_SIZE)
				return null;

			for (i = 0; i < MMU_TABLE_NUM; i++)
				if (MMU[i].usageFlag == true)
				{
					mem_used += MMU[i].memSize;
					mmu_tab_used_index[mmu_tab_used_num++] = MMU[i].tabIndex;
				}

			if (MEMORY_SIZE < (mem_used + size))
				return null;

			if (mem_used == 0)
			{
				MMU[1].pmem = MMU[0].pmem;
				MMU[1].memSize = size;
				MMU[1].usageFlag = true;
				return MMU[1];
			}

			for (i = 0; i < (mmu_tab_used_num - 1); i++)
				for (j = 0; j < (mmu_tab_used_num - i - 1); j++)
					if (MMU[mmu_tab_used_index[j]].pmem > MMU[mmu_tab_used_index[j + 1]].pmem)
					{
						mmu_tab_used_index[j + 1] ^= mmu_tab_used_index[j];
						mmu_tab_used_index[j] ^= mmu_tab_used_index[j + 1];
						mmu_tab_used_index[j + 1] ^= mmu_tab_used_index[j];
					}

			for (i = 0; i < (mmu_tab_used_num - 1); i++)
			{
				temp32 = (MMU[mmu_tab_used_index[i + 1]].pmem - 
					MMU[mmu_tab_used_index[i]].pmem);

				if ((temp32 - MMU[mmu_tab_used_index[i]].memSize) >= size)
				{
					if (temp_area[1] > (temp32 - MMU[mmu_tab_used_index[i]].memSize))
					{
						temp_area[0] = MMU[mmu_tab_used_index[i]].tabIndex;
						temp_area[1] = temp32 - MMU[mmu_tab_used_index[i]].memSize;
					}
				}

				record_temp32 += temp32 - MMU[mmu_tab_used_index[i]].memSize;
				if (record_temp32 >= size && magic_flag == 0)
				{
					j = i;
					magic_flag = 1;
				}
			}

			if (temp_area[0] == int.MaxValue && temp_area[1] == int.MaxValue)
			{
				for (i = 0; i < j; i++)
				{
					if (MMU[mmu_tab_used_index[i + 1]].pmem > 
						(MMU[mmu_tab_used_index[i]].pmem + MMU[mmu_tab_used_index[i]].memSize))
					{

						MMU[mmu_tab_used_index[i + 1]].pmem.Move(
						MMU[mmu_tab_used_index[i]].pmem + MMU[mmu_tab_used_index[i]].memSize,
						MMU[mmu_tab_used_index[i + 1]].memSize);
						MMU[mmu_tab_used_index[i + 1]].pmem =
						  MMU[mmu_tab_used_index[i]].pmem + MMU[mmu_tab_used_index[i]].memSize;
					}
				}

				for (i = 1; i < (MMU_TABLE_NUM - 1); i++)
				{
					if (MMU[i].usageFlag == false)
					{
						MMU[i].pmem =
						  MMU[mmu_tab_used_index[j]].pmem + MMU[mmu_tab_used_index[j]].memSize;

						MMU[i].memSize = size;
						MMU[i].usageFlag = true;
						return MMU[i];
					}
				}
				return null;
			}

			for (i = 1; i < (MMU_TABLE_NUM - 1); i++)
			{
				if (MMU[i].usageFlag == false)
				{
					MMU[i].pmem = MMU[temp_area[0]].pmem + MMU[temp_area[0]].memSize;

					MMU[i].memSize = size;
					MMU[i].usageFlag = true;
					return MMU[i];
				}
			}

			return null;
		}

		void setupSession()
		{
			for (var i = 0; i < SESSION_TABLE_NUM; i++)
			{
				CMDSessionTab[i] = new CMDSession();

				CMDSessionTab[i].sessionID = (byte)(i);
				CMDSessionTab[i].usageFlag = false;
				CMDSessionTab[i].mmu = null;
			}

			for (var i = 0; i < (SESSION_TABLE_NUM - 1); i++)
			{
				ACKSessionTab[i] = new ACKSession();

				ACKSessionTab[i].sessionID = (byte)(i + 1);
				ACKSessionTab[i].sessionStatus = ACK_SESSION_IDLE;
				ACKSessionTab[i].mmu = null;
			}
		}

		/*! @note Alloc a cmd session for sending cmd data
		 *  when arg session_id = 0/1, it means select session 0/1 to send cmd
		 *  otherwise set arg session_id = CMD_SESSION_AUTO (32), which means auto
		 *  select a idle session id is between 2~31.
		 */

		CMDSession allocSession(ushort session_id, 
			int size)
		{
			uint i;
			API_LOG(serialDevice, DEBUG_LOG, "Allocation size {0}", size);
			MMU_Tab mmu = null;

			if (session_id == 0 || session_id == 1)
			{
				if (CMDSessionTab[session_id].usageFlag == false)
					i = session_id;
				else
				{
					/* session is busy */
					API_LOG(serialDevice, ERROR_LOG, "session {0} is busy", session_id);
					return null;
				}
			}
			else
			{
				for (i = 2; i < SESSION_TABLE_NUM; i++)
					if (CMDSessionTab[i].usageFlag == false)
						break;
			}
			if (i < 32 && CMDSessionTab[i].usageFlag == false)
			{
				CMDSessionTab[i].usageFlag = true;
				mmu = allocMemory(size);
				if (mmu == null)
					CMDSessionTab[i].usageFlag = false;
				else
				{
					CMDSessionTab[i].mmu = mmu;
					return CMDSessionTab[i];
				}
			}
			return null;
		}

		void freeSession(CMDSession session)
		{
			if (session.usageFlag == true)
			{
				API_LOG(serialDevice, DEBUG_LOG, "session id {0}", session.sessionID);
				freeMemory(session.mmu);
				session.usageFlag = false;
			}
		}

		ACKSession allocACK(ushort session_id, int size)
		{
			MMU_Tab mmu = null;
			if (session_id > 0 && session_id < 32)
			{
				if (ACKSessionTab[session_id - 1].mmu != null)
					freeACK(ACKSessionTab[session_id - 1]);
				mmu = allocMemory(size);
				if (mmu == null)
				{
					API_LOG(serialDevice, ERROR_LOG, "there is not enough memory");
					return null;
				}
				else
				{
					ACKSessionTab[session_id - 1].mmu = mmu;
					return ACKSessionTab[session_id - 1];
				}
			}
			API_LOG(serialDevice, ERROR_LOG, "wrong Ack session ID: 0x{0:X}", session_id);
			return null;
		}

		void freeACK(ACKSession session) { freeMemory(session.mmu); }
	}
}
