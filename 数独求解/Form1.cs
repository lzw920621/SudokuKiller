using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

namespace 数独求解
{
    public partial class Form1 : Form
    {
        Sudoku sudoku;
        

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            InitialGrid();
        }

        void InitialGrid()//初始化表格
        {
            this.dataGridView1.Size = new Size(450, 450);//长宽

            this.dataGridView1.Paint += DataGrid_Paint;//绑定重绘事件
            this.dataGridView1.AllowUserToAddRows = false;//不允许添加行
            this.dataGridView1.AllowUserToDeleteRows = false;//不允许删除行
            this.dataGridView1.AllowDrop = false;//
            this.dataGridView1.AllowUserToOrderColumns = false;
            this.dataGridView1.AllowUserToResizeColumns = false;
            this.dataGridView1.AllowUserToResizeRows = false;
            this.dataGridView1.ReadOnly = true;

            this.dataGridView1.ColumnHeadersVisible = false;//不显示列标题
            this.dataGridView1.RowHeadersVisible = false;//不显示行标题
            this.dataGridView1.ScrollBars = ScrollBars.None;//不显示滚动条
            

            //dataGridView1.DefaultCellStyle.SelectionBackColor = Color.White;//设置表格元素选中后背景色设为白色 
            //dataGridView1.DefaultCellStyle.SelectionForeColor = SystemColors.ControlText;

            this.dataGridView1.DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            this.dataGridView1.Rows.Add(9);//添加9行
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                dataGridView1.Rows[i].Height = 50;//单元格高度
            }
            for (int i = 0; i < dataGridView1.ColumnCount; i++)
            {
                dataGridView1.Columns[i].Width = 50;//单元格宽度
            }
            dataGridView1.ClearSelection();//取消默认选定
        }

        void DataGrid_Paint(object sender, PaintEventArgs e)//
        {
            e.Graphics.DrawLine(new Pen(Color.Black), 150, 0, 150, 450);
            e.Graphics.DrawLine(new Pen(Color.Black), 300, 0, 300, 450);
           
            e.Graphics.DrawLine(new Pen(Color.Black), 0, 150, 450, 150);
            e.Graphics.DrawLine(new Pen(Color.Black), 0, 300, 450, 300);            
        }

        private void button_Num_Click(object sender, EventArgs e)
        {
            string strNum = (sender as Button).Text;
            WriteNum(strNum);           
        }

        private void WriteNum(string strNum)
        {
            if(dataGridView1.SelectedCells.Count==1)
            {
                dataGridView1.SelectedCells[0].Value = strNum;
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (dataGridView1.SelectedCells.Count == 1)
            {
                if (dataGridView1.SelectedCells[0].Value == null)
                {
                    ResetAllCellsBackColor();
                    return;
                }
                string currentCellNum = dataGridView1.SelectedCells[0].Value as string;//当前选中的单元格内的数字
                for (int i = 0; i < dataGridView1.RowCount; i++)
                {
                    for (int j = 0; j < dataGridView1.ColumnCount; j++)
                    {
                        if(dataGridView1.Rows[i].Cells[j].Value as string ==currentCellNum)
                        {
                            dataGridView1.Rows[i].Cells[j].Style.BackColor = Color.Lime;
                        }   
                        else
                        {
                            dataGridView1.Rows[i].Cells[j].Style.BackColor = SystemColors.Window;
                        }
                    }
                }
            }
            
        }

        private void ResetAllCellsBackColor()//恢复所有单元格的背景色
        {
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                for (int j = 0; j < dataGridView1.ColumnCount; j++)
                {
                    if(dataGridView1.Rows[i].Cells[j].Style.BackColor!= SystemColors.Window)
                    {
                        dataGridView1.Rows[i].Cells[j].Style.BackColor = SystemColors.Window;
                    }                  
                }
            }
        }

        private void ResetAllCells()
        {
            for (int i = 0; i < dataGridView1.RowCount; i++)
            {
                for (int j = 0; j < dataGridView1.ColumnCount; j++)
                {
                    dataGridView1.Rows[i].Cells[j].Value = null;
                    dataGridView1.Rows[i].Cells[j].Style.BackColor = SystemColors.Window;
                }
            }
        }

        private void HighlightSelectedNum()//高亮选中的数字
        {
            if (dataGridView1.SelectedCells.Count == 1)
            {
                if (dataGridView1.SelectedCells[0].Value == null)//如果选中的单元格为空
                {
                    ResetAllCellsBackColor();
                    return;
                }
                string currentCellNum = dataGridView1.SelectedCells[0].Value as string;//当前选中的单元格内的数字
                for (int i = 0; i < dataGridView1.RowCount; i++)
                {
                    for (int j = 0; j < dataGridView1.ColumnCount; j++)
                    {
                        if (dataGridView1.Rows[i].Cells[j].Value as string == currentCellNum)
                        {
                            dataGridView1.Rows[i].Cells[j].Style.BackColor = Color.Lime;
                        }
                        else
                        {
                            dataGridView1.Rows[i].Cells[j].Style.BackColor = SystemColors.Window;
                        }
                    }
                }
            }
        }

        private void button_Reset_Click(object sender, EventArgs e)
        {
            ResetAllCells();
        }

        private void button_Solove_Click(object sender, EventArgs e)
        {
            sudoku = new Sudoku(GetArray());
            sudoku.SendMessage += this.MessageCallback;
            sudoku.UpdateCell += this.UpdateCellCallback;
            sudoku.UpdateGrid += this.UpdateGridCallback;
            //Task.Run(new Action(() => { sudoku.GetAnswer(); }));
            new Thread(() => { sudoku.GetAnswer(); }).Start();
        }

        private void UpdateGridCallback(Cell[,] cells)
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    dataGridView1.Rows[i].Cells[j].Value = cells[i, j].Value + "";
                }
            }
        }

        private int[,] GetArray()
        {
            int[,] array = new int[9, 9];
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if(dataGridView1.Rows[i].Cells[j].Value == null)
                    {
                        array[i, j] = 0;
                    }
                    else
                    {
                        array[i, j] = int.Parse(dataGridView1.Rows[i].Cells[j].Value as string);
                    }
                }
            }
            return array;
        }

        void MessageCallback(string msg)//回调函数 业务层的消息
        {          
            this.BeginInvoke(new Action(() => { MessageBox.Show(msg); }));
        }

        void UpdateCellCallback(Cell cell)
        {
            if(cell.assigned==true)
            {             
                this.BeginInvoke(new Action(() => 
                {
                    dataGridView1.Rows[cell.rowIndex].Cells[cell.colomIndex].Value = cell.Value + "";
                }));
            }           
        }
        
    }
}
