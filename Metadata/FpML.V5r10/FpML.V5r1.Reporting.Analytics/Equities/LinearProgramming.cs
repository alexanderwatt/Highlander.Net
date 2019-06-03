using System;
using System.Collections.Generic;
using System.Text;

namespace Orion.Analytics.Equities
{

     // The linear programming classes reside in their own namespace.
    using Extreme.Mathematics.Optimization.LinearProgramming;
    // Vectors and matrices are in the Extreme.Mathematics.LinearAlgebra
    using Extreme.Mathematics.LinearAlgebra.Sparse;
    // namespace
    using Extreme.Mathematics.LinearAlgebra;

    /// <summary>
    /// Illustrates solving linear programming problems
    /// using the classes in the Extreme.Mathematics.Optimization.LinearProgramming
    /// namespace of the Extreme Optimization Numerical Libraries for .NET.
    /// </summary>
    public class LinearProgramming
     {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        /// 

        const double cCapProp1 = 0.10;
        const double cCapProp2 = 0.05; 
        const double cCapProp3 = 0.5;
        const double cEpsilon = 0;

        private List<Stock> _Portfolio;

        private int _stockCount;
        
        public List<Stock> Portfolio
        {
          get { return _Portfolio; }
          set {_Portfolio=value;}
        }

        public int StockCount
        {
          get { return _stockCount; }
          set {_stockCount=value;}          
        }
             

        public void AddMyConstraints(ref LinearProgram lp, int condition)
        {

          double stockval;
          string id;

          int idx = 0;
          foreach (Stock stock in Portfolio)
          {
            idx++;
            // Next, we add two variables: we specify the name, the cost,
            // and optionally the lower and upper bound.              
            id = stock.StockName + idx;
            stockval = stock.Dollars;
            lp.AddVariable(id, 1.0, 0, stockval);
          }

          _stockCount = idx;

          /* Constraint set 1 - x1 > x2> x3 >... > xn*/
          GeneralVector aux = new GeneralVector(_stockCount);
          for (int jdx = 0; jdx < _stockCount - 1; jdx++)
          {
            id = "C" + jdx;
            for (idx = 0; idx < _stockCount; idx++)
            {
              if (idx == jdx)
              {
                aux[idx] = 1;
              }
              else if (idx == jdx + 1)
              {
                aux[idx] = -1;
              }
              else
              {
                aux[idx] = 0;
              }
            }
            lp.AddConstraint(id, aux, LinearProgramConstraintType.GreaterThanOrEqualConstraint, 0);
          }

          /*
           * Calculates for 50% of # of stocks rather than exposure
          for (int jdx = stockCount; jdx < 2*stockCount; jdx++)
          {
            double temp = (double)stockCount / 2;
            int pc50 = stockCount -1 + (int)Math.Floor(temp);
            id = "C" + jdx;
            for (idx = 0; idx < stockCount; idx++)
            {
              if (jdx <= pc50)
              {
                if (idx + stockCount == jdx)
                {
                  aux[idx] = cCapProp1 - 1;
                }
                else
                {
                  aux[idx] = cCapProp1;
                }
              }
              else
              {
                if (idx + stockCount == jdx)
                {
                  aux[idx] = cCapProp2 - 1;
                }
                else
                {
                  aux[idx] = cCapProp2;
                }
              }
            }
            lp.AddConstraint(id, aux, LinearProgramConstraintType.GreaterThanOrEqualConstraint, 0);
          }
            
          */
          /*x1<0.1(x1+...xn)*/

          id = "D0";
          for (idx = 0; idx < _stockCount; idx++)
          {
            if (idx == 0)
            {
              aux[idx] = cCapProp1 - 1;
            }
            else
            {
              aux[idx] = cCapProp1;
            }
          }
          lp.AddConstraint(id, aux, LinearProgramConstraintType.GreaterThanOrEqualConstraint, 0);


          /* x1+x2+x3+x4+x[condition]<= 0.5(x1+...+xn) */
          id = "D1";
          for (idx = 0; idx < _stockCount; idx++)
          {
            if (idx < condition)
            {
              aux[idx] = cCapProp3 - 1;
            }
            else
            {
              aux[idx] = cCapProp3;
            }         
          }
          lp.AddConstraint(id, aux, LinearProgramConstraintType.GreaterThanOrEqualConstraint, 0);

          /* x[condition] >= 0.05(x1+..xn)*/
          /*
          id = "D2";
          for (idx = 0; idx < _stockCount; idx++)
          {
            if (idx == condition - 1)
            {
              aux[idx] = 1 - cCapProp2;
            }
            else
            {
              aux[idx] = -cCapProp2;
            }
          }
          lp.AddConstraint(id, aux, LinearProgramConstraintType.GreaterThanOrEqualConstraint, 0);*/

          /*x[condition+1] <= (0.05-epsilon)(x1+..xn) */
          id = "D3";
          for (idx = 0; idx < _stockCount; idx++)
          {
            if (idx == condition)
            {
              aux[idx] = cCapProp2-cEpsilon - 1;
            }
            else
            {
              aux[idx] = cCapProp2-cEpsilon;
            }
          }
          lp.AddConstraint(id, aux, LinearProgramConstraintType.GreaterThanOrEqualConstraint, 0);                              
                          
        }

         [STAThread]
        public double[] CalcFinalOptimum()
        {   
         
          int maxidx=0;

          double temp = 0;          
          double[] solution = new double[_stockCount];

          for (int idx = 5; idx<=10;idx++)
          {
            solution = this.CalcOptimum(idx);
            double x = SumArray(solution);
            if (SumArray(solution)>temp)
            {
              temp=SumArray(solution);
              maxidx = idx;
            }
          }
          return CalcOptimum(maxidx);
        }    


        public double SumArray(double[] arr)
        {
          double sum = 0;
          for (int idx = 0; idx < arr.Length; idx++)
          {
             sum += arr[idx];
          }
          return sum;
        }

      [STAThread]
      public double[] CalcOptimum(int condition)
        {
            // Condition refers to a number 5 to 10 s.t 
            // x1+x2+x3+x4+x[condition]<= 0.5(x1+...+xn)   
            // This QuickStart sample illustrates the three ways to create a Linear
            // Program. The first is in terms of matrices. The coefficients
            // are supplied as a matrix. The cost vector, right-hand side
            // and constraints on the variables are supplied as a vector.
           // static void Main(string[] args)
           /*
            *             
      
            * */
            // The second way to create a Linear Program is by constructing
            // it by hand. We start with an 'empty' linear program.         
                                             
            
            // Next, we add constraints. Constraints also have a name.
            // We also specify the coefficients of the variables,
            // the lower bound and the upper bound.

            LinearProgram lp = new LinearProgram();

            AddMyConstraints(ref lp, condition);

            double[] _x = new double[_stockCount];
        
            // If a constraint is a simple equality or inequality constraint,
            // you can supply a LinearProgramConstraintType value and the
            // right-hand side of the constraint.

            // We can now solve the linear program:   
            lp.ExtremumType = Extreme.Mathematics.Optimization.ExtremumType.Maximum;
            Vector x = lp.Solve();
            Vector y = lp.GetDualSolution();
            //Console.WriteLine("Primal: {0:F1}", x);
            //Console.WriteLine("Dual:   {0:F1}", y);
            //Console.WriteLine("Optimal value:   {0:F1}", lp.Extremum);
            //Console.Read();
            //Return a double;         

            if ((lp.Status == LinearProgramStatus.Infeasible) |
                (lp.Status == LinearProgramStatus.Unknown) |
                (lp.Status == LinearProgramStatus.Unbounded))
            {
              foreach (Stock stock in Portfolio)
              {
                int idx = 0;
                _x[idx++] = stock.Dollars;
              }
              return _x;
            }
            else
            {
              x.CopyTo(_x);
              return _x;
            } 

            // Finally, we can create a linear program from an MPS file.
            // The MPS format is a standard format.
           // LinearProgram lp3 = MpsReader.Read(@"..\..\sample.mps");
            // We can go straight to solving the linear program:
            /*x = lp2.Solve();
            y = lp2.GetDualSolution();
            Console.WriteLine("Primal: {0:F1}", x);
            Console.WriteLine("Dual:   {0:F1}", y);
            Console.WriteLine("Optimal value:   {0:F1}", lp2.Extremum);

            Console.Write("Press Enter key to exit...");
            Console.ReadLine();*/
        }
    }
}
  

