﻿using System;
using System.IO;
using System.Diagnostics;

/// <summary>
/// Matrix 的摘要说明。
/// 实现矩阵的基本运算
/// </summary>
public class Matrix
{

    //构造方阵
    public Matrix(int row)
    {
        m_data = new double[row, row];

    }
    public Matrix(int row, int col)
    {
        m_data = new double[row, col];
    }
    //复制构造函数
    public Matrix(Matrix m)
    {
        int row = m.Row;
        int col = m.Col;
        m_data = new double[row, col];

        for (int i = 0; i < row; i++)
            for (int j = 0; j < col; j++)
                m_data[i, j] = m[i, j];

    }

    //unit matrix:设为单位阵
    public void SetUnit()
    {
        for (int i = 0; i < m_data.GetLength(0); i++)
            for (int j = 0; j < m_data.GetLength(1); j++)
                m_data[i, j] = ((i == j) ? 1 : 0);
    }

    //设置元素值
    public void SetValue(double d)
    {
        for (int i = 0; i < m_data.GetLength(0); i++)
            for (int j = 0; j < m_data.GetLength(1); j++)
                m_data[i, j] = d;
    }

    // Value extraction：返中行数
    public int Row
    {
        get
        {

            return m_data.GetLength(0);
        }
    }

    //返回列数
    public int Col
    {
        get
        {
            return m_data.GetLength(1);
        }
    }

    //重载索引
    //存取数据成员
    public double this[int row, int col]
    {
        get
        {
            return m_data[row, col];
        }
        set
        {
            m_data[row, col] = value;
        }
    }

    //primary change
    //　初等变换　对调两行：ri<-->rj
    public Matrix Exchange(int i, int j)
    {
        double temp;

        for (int k = 0; k < Col; k++)
        {
            temp = m_data[i, k];
            m_data[i, k] = m_data[j, k];
            m_data[j, k] = temp;
        }
        return this;
    }


    //初等变换　第index 行乘以mul
    Matrix Multiple(int index, double mul)
    {
        for (int j = 0; j < Col; j++)
        {
            m_data[index, j] *= mul;
        }
        return this;
    }


    //初等变换 第src行乘以mul加到第index行
    Matrix MultipleAdd(int index, int src, double mul)
    {
        for (int j = 0; j < Col; j++)
        {
            m_data[index, j] += m_data[src, j] * mul;
        }

        return this;
    }

    //transpose 转置
    public Matrix Transpose()
    {
        Matrix ret = new Matrix(Col, Row);

        for (int i = 0; i < Row; i++)
            for (int j = 0; j < Col; j++)
            {
                ret[j, i] = m_data[i, j];
            }
        return ret;
    }

    //binary addition 矩阵加
    public static Matrix operator +(Matrix lhs, Matrix rhs)
    {
        if (lhs.Row != rhs.Row)    //异常
        {
            System.Exception e = new Exception("相加的两个矩阵的行数不等");
            throw e;
        }
        if (lhs.Col != rhs.Col)     //异常
        {
            System.Exception e = new Exception("相加的两个矩阵的列数不等");
            throw e;
        }

        int row = lhs.Row;
        int col = lhs.Col;
        Matrix ret = new Matrix(row, col);

        for (int i = 0; i < row; i++)
            for (int j = 0; j < col; j++)
            {
                double d = lhs[i, j] + rhs[i, j];
                ret[i, j] = d;
            }
        return ret;

    }

    //binary subtraction 矩阵减
    public static Matrix operator -(Matrix lhs, Matrix rhs)
    {
        if (lhs.Row != rhs.Row)    //异常
        {
            System.Exception e = new Exception("相减的两个矩阵的行数不等");
            throw e;
        }
        if (lhs.Col != rhs.Col)     //异常
        {
            System.Exception e = new Exception("相减的两个矩阵的列数不等");
            throw e;
        }

        int row = lhs.Row;
        int col = lhs.Col;
        Matrix ret = new Matrix(row, col);

        for (int i = 0; i < row; i++)
            for (int j = 0; j < col; j++)
            {
                double d = lhs[i, j] - rhs[i, j];
                ret[i, j] = d;
            }
        return ret;
    }


    //binary multiple 矩阵乘
    public static Matrix operator *(Matrix lhs, Matrix rhs)
    {
        if (lhs.Col != rhs.Row)    //异常
        {
            System.Exception e = new Exception("相乘的两个矩阵的行列数不匹配");
            throw e;
        }
        Matrix ret = new Matrix(lhs.Row, rhs.Col);
        double temp;
        for (int i = 0; i < lhs.Row; i++)
        {
            for (int j = 0; j < rhs.Col; j++)
            {
                temp = 0;
                for (int k = 0; k < lhs.Col; k++)
                {
                    temp += lhs[i, k] * rhs[k, j];
                }
                ret[i, j] = temp;
            }
        }

        return ret;
    }


    //binary division 矩阵除
    public static Matrix operator /(Matrix lhs, Matrix rhs)
    {
        return lhs * rhs.Inverse();
    }

    //unary addition单目加
    public static Matrix operator +(Matrix m)
    {
        Matrix ret = new Matrix(m);
        return ret;
    }

    //unary subtraction 单目减
    public static Matrix operator -(Matrix m)
    {
        Matrix ret = new Matrix(m);
        for (int i = 0; i < ret.Row; i++)
            for (int j = 0; j < ret.Col; j++)
            {
                ret[i, j] = -ret[i, j];
            }

        return ret;
    }

    //number multiple 数乘
    public static Matrix operator *(double d, Matrix m)
    {
        Matrix ret = new Matrix(m);
        for (int i = 0; i < ret.Row; i++)
            for (int j = 0; j < ret.Col; j++)
                ret[i, j] *= d;

        return ret;
    }

    //number division 数除
    public static Matrix operator /(double d, Matrix m)
    {
        return d * m.Inverse();
    }

    //求逆  
    public Matrix Inverse()
    {
        
        //clone  
        Matrix a = new Matrix(this);
        
        Matrix c = new Matrix(Row, Row);
        c.SetUnit();

        //i表示第几行，j表示第几列  
        for (int j = 0; j < Row; j++)
        {
            bool flag = false;
            for (int i = j; i < Col; i++)
            {
                if (a[i, j] != 0)
                {
                    flag = true;
                    double temp;
                    //交换i,j,两行  
                    if (i != j)
                    {
                        for (int k = 0; k < Col; k++)
                        {
                            temp = a[j, k];
                            a[j, k] = a[i, k];
                            a[i, k] = temp;

                            temp = c[j, k];
                            c[j, k] = c[i, k];
                            c[i, k] = temp;
                        }
                    }
                    //第j行标准化  
                    double d = a[j, j];
                    for (int k = 0; k < Col; k++)
                    {
                        a[j, k] = a[j, k] / d;
                        c[j, k] = c[j, k] / d;
                    }
                    //消去其他行的第j列  
                    d = a[j, j];
                    for (int k = 0; k < Col; k++)
                    {
                        if (k != j)
                        {
                            double t = a[k, j];
                            for (int n = 0; n < Col; n++)
                            {
                                a[k, n] -= (t / d) * a[j, n];
                                c[k, n] -= (t / d) * c[j, n];
                            }
                        }
                    }
                }
            }
            if (!flag) return null;
        }
        return c;
    }  
    //功能：返回列主元素的行号
    //参数：row为开始查找的行号
    //说明：在行号[row,Col)范围内查找第row列中绝对值最大的元素，返回所在行号
    int Pivot(int row)
    {
        int index = row;

        for (int i = row + 1; i < Row; i++)
        {
            if (m_data[i, row] > m_data[index, row])
                index = i;
        }

        return index;
    }
    
    //determine if the matrix is square:方阵
    public bool IsSquare()
    {
        return Row == Col;
    }

    //determine if the matrix is symmetric对称阵
    public bool IsSymmetric()
    {

        if (Row != Col)
            return false;

        for (int i = 0; i < Row; i++)
            for (int j = i + 1; j < Col; j++)
                if (m_data[i, j] != m_data[j, i])
                    return false;

        return true;
    }

    //一阶矩阵->实数
    public double ToDouble()
    {
        Trace.Assert(Row == 1 && Col == 1);

        return m_data[0, 0];
    }

    //conert to string
    public override string ToString()
    {

        string s = "";
        for (int i = 0; i < Row; i++)
        {
            for (int j = 0; j < Col; j++)
                s += string.Format("{0} ", m_data[i, j]);

            s += "\r\n";
        }
        return s;

    }

    public Matrix Solve(Matrix b)
    {
        Trace.Assert(IsSquare() && Row ==b.Row);
        return Inverse() * b;
    }

    public Matrix LsmFitting(Matrix b)
    {
        Trace.Assert(Row == b.Row);
        Matrix m = Transpose() * this;
        return m.Solve(Transpose() * b);
    }
    //私有数据成员
    private double[,] m_data;

}//end class Matrix