using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace 数独求解
{
    class Sudoku
    {
        Cell[,] cells;
        Dictionary<int, List<Cell>> blockDic;
        List<Cell> unassignedCells;//未确定值的格子

        public event Action<string> SendMessage;//向界面发送消息

        public event Action<Cell[,]> UpdateGrid;//更新界面显示

        public event Action<Cell> UpdateCell;//更新单元格

        //构造函数
        public Sudoku(int[,] array)
        {
            if(array.GetLength(0)!=9 || array.GetLength(1) != 9)
            {
                throw new Exception("数独初始化行列数都必须为9");
            }
            this.cells = new Cell[9, 9];
            this.blockDic = new Dictionary<int, List<Cell>>();
            this.unassignedCells = new List<Cell>();
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Cell tempCell= new Cell(array[i, j], i, j);
                    
                    if (tempCell.Value == 0)//未确定值的格子
                    {
                        unassignedCells.Add(tempCell);
                    }
                    int blockIndex = tempCell.blockIndex;
                    if(blockDic.ContainsKey(blockIndex))
                    {
                        blockDic[blockIndex].Add(tempCell);
                    }
                    else
                    {
                        blockDic[blockIndex] = new List<Cell> { tempCell };
                    }
                    tempCell.ValueAssigned += this.ValueAssignedCallback;

                    this.cells[i, j] = tempCell;
                }
            }
        }       

        public void GetAnswer()
        {
            bool changed = true;
            while(changed)
            {
                changed = false;
                for (int i = 0; i < 9; i++)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        if (cells[i, j].assigned == true)
                        {                          
                            if (RowEliminate(cells[i, j])==true)
                            {
                                changed = true;
                            }
                            if(ColomnEliminate(cells[i, j])==true)
                            {
                                changed = true;
                            }
                            if(BlockEliminate(cells[i, j])==true)
                            {
                                changed = true;
                            }
                        }
                    }
                }
            }
            bool result=Backtrace(0, unassignedCells);
            if(result==true)
            {
                UpdateGrid(this.cells);
            }           
        }

        private bool Backtrace(int index,List<Cell> unassignedCells)
        {
            if(index==unassignedCells.Count)
            {
                return true;
            }
            Cell tempCell = unassignedCells[index];
            for (int i = 0; i < tempCell.candidates.Count; i++)
            {
                tempCell.Value = tempCell.candidates[i];
                tempCell.assigned = true;
                if(IsRowValid(tempCell) && IsColomnValid(tempCell) && IsBlockValid(tempCell) && Backtrace(index + 1, unassignedCells))
                {
                    return true;
                }               
                tempCell.Value = 0;
                tempCell.assigned = false;
            }
            return false;
        }
        //判断当前格子的值与该行的其他格子是否有冲突
        private bool IsRowValid(Cell currentCell)
        {
            int rowIndex = currentCell.rowIndex;
            for (int i = 0; i < 9; i++)
            {
                if (cells[rowIndex, i] != currentCell && cells[rowIndex, i].Value == currentCell.Value)
                {
                    return false;
                }
            }
            return true;
        }
        //判断当前格子的值与该列的其他格子是否有冲突
        private bool IsColomnValid(Cell currentCell)
        {
            int colomnIndex = currentCell.colomIndex;
            for (int i = 0; i < 9; i++)
            {
                if(cells[i,colomnIndex]!=currentCell && cells[i,colomnIndex].Value==currentCell.Value)
                {
                    return false;
                }
            }
            return true;
        }
        //判断当前格子的值与所在宫的其他格子是否有冲突
        private bool IsBlockValid(Cell currentCell)
        {
            List<Cell> tempList = blockDic[currentCell.blockIndex];
            foreach (var cell in tempList)
            {
                if (cell != currentCell && cell.Value == currentCell.Value)
                {
                    return false;
                }
            }
            return true;
        }



        //行排除
        private bool RowEliminate(Cell currentCell)
        {
            bool flag = false;
            int rowIndex = currentCell.rowIndex;
            for (int i = 0; i < 9; i++)
            {
                if(cells[rowIndex,i]!=currentCell && cells[rowIndex,i].assigned==false)
                {
                    if(cells[rowIndex, i].DeleteNumFromCandidates(currentCell.Value))
                    {
                        flag = true;
                    }
                }
            }
            return flag;
        }
        //列排除
        private bool ColomnEliminate(Cell currentCell)
        {
            bool flag = false;
            int colomnIndex = currentCell.colomIndex;
            for (int i = 0; i < 9; i++)
            {
                if(cells[i,colomnIndex]!=currentCell && cells[i, colomnIndex].assigned==false)
                {
                    if(cells[i, colomnIndex].DeleteNumFromCandidates(currentCell.Value))
                    {
                        flag = true;
                    }
                }
            }
            return flag;
        }
        //宫排除
        private bool BlockEliminate(Cell currentCell)
        {
            bool flag = false;
            List<Cell> tempList = blockDic[currentCell.blockIndex];
            foreach (var cell in tempList)
            {
                if(cell!=currentCell && cell.assigned==false)
                {
                    if(cell.DeleteNumFromCandidates(currentCell.Value))
                    {
                        flag = true;
                    }
                }
            }
            return flag;
        }

        //
        private void ValueAssignedCallback(Cell cell)
        {
            unassignedCells.Remove(cell);//将已确定值的格子从待定格子中删除
            UpdateCell(cell);
        }

    }
    class Cell
    {
        public int Value { get; set; }//值
        public List<int> candidates;//候选数
        public bool assigned { get; set; }//值已是否已确定

        public int rowIndex;//行索引
        public int colomIndex;//列索引
        public int blockIndex;//宫索引

        public event Action<Cell> ValueAssigned;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="num">值</param>
        /// <param name="i">行</param>
        /// <param name="j">列</param>
        public Cell(int num,int i,int j)
        {
            this.Value = num;
            this.rowIndex = i;//行索引
            this.colomIndex = j;//列索引
            this.blockIndex = i / 3 * 3 + j / 3;//宫索引
            if(num==0)
            {
                this.assigned = false;
                this.candidates = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            }
            else
            {
                this.assigned = true;
                this.candidates = null;
            }          
        }

        public bool DeleteNumFromCandidates(int num)
        {
            bool result=candidates.Remove(num);
            if(candidates.Count==1)
            {
                this.Value = candidates.First();
                this.assigned = true;
                ValueAssigned(this);
            }
            return result;
        }
    }
}
