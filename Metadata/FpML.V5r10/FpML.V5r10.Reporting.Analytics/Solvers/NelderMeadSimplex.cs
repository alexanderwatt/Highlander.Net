using System;
using Orion.Analytics.Maths.Collections;

namespace Orion.Analytics.Solvers
{
    /// <summary>
    /// Class implementing the Nelder-Mead simplex algorithm, used to find a minima when no gradient is available.
    /// Called fminsearch() in Matlab. A description of the algorithm can be found at
    /// http://se.mathworks.com/help/matlab/math/optimizing-nonlinear-functions.html#bsgpq6p-11
    /// or
    /// https://en.wikipedia.org/wiki/Nelder%E2%80%93Mead_method
    /// </summary>
    public sealed class NelderMeadSimplex : IUnconstrainedMinimizer
    {
        static readonly double JITTER = 1e-10d;           // a small value used to protect against floating point noise
        public double ConvergenceTolerance { get; set; }
        public int MaximumIterations { get; set; }

        public NelderMeadSimplex(double convergenceTolerance, int maximumIterations)
        {
            ConvergenceTolerance = convergenceTolerance;
            MaximumIterations = maximumIterations;
        }

        /// <summary>
        /// Finds the minimum of the objective function without an intial pertubation, the default values used
        /// by fminsearch() in Matlab are used instead
        /// http://se.mathworks.com/help/matlab/math/optimizing-nonlinear-functions.html#bsgpq6p-11
        /// </summary>
        /// <param name="objectiveFunction">The objective function, no gradient or hessian needed</param>
        /// <param name="initialGuess">The intial guess</param>
        /// <returns>The minimum point</returns>
        public MinimizationResult FindMinimum(IMultiObjectiveFunction objectiveFunction, DoubleVector initialGuess)
        {
            return Minimum(objectiveFunction, initialGuess, ConvergenceTolerance, MaximumIterations);
        }

        /// <summary>
        /// Finds the minimum of the objective function with an intial pertubation
        /// </summary>
        /// <param name="objectiveFunction">The objective function, no gradient or hessian needed</param>
        /// <param name="initialGuess">The intial guess</param>
        /// <param name="initalPertubation">The inital pertubation</param>
        /// <returns>The minimum point</returns>
        public MinimizationResult FindMinimum(IMultiObjectiveFunction objectiveFunction, DoubleVector initialGuess, DoubleVector initalPertubation)
        {
            return Minimum(objectiveFunction, initialGuess, initalPertubation, ConvergenceTolerance, MaximumIterations);
        }

        /// <summary>
        /// Finds the minimum of the objective function without an intial pertubation, the default values used
        /// by fminsearch() in Matlab are used instead
        /// http://se.mathworks.com/help/matlab/math/optimizing-nonlinear-functions.html#bsgpq6p-11
        /// </summary>
        /// <param name="objectiveFunction">The objective function, no gradient or hessian needed</param>
        /// <param name="initialGuess">The intial guess</param>
        /// <param name="convergenceTolerance"></param>
        /// <param name="maximumIterations"></param>
        /// <returns>The minimum point</returns>
        public static MinimizationResult Minimum(IMultiObjectiveFunction objectiveFunction, DoubleVector initialGuess, double convergenceTolerance = 1e-8, int maximumIterations = 1000)
        {
            var initalPertubation = new DoubleVector(initialGuess.Count);
            for (int i = 0; i < initialGuess.Count; i++)
            {
                initalPertubation[i] = Math.Abs(initialGuess[i]) > 0.0 ? initialGuess[i] * 0.05 : 0.00025;
            }
            return Minimum(objectiveFunction, initialGuess, initalPertubation, convergenceTolerance, maximumIterations);
        }

        /// <summary>
        /// Finds the minimum of the objective function with an intial pertubation
        /// </summary>
        /// <param name="objectiveFunction">The objective function, no gradient or hessian needed</param>
        /// <param name="initialGuess">The intial guess</param>
        /// <param name="initalPertubation">The inital pertubation</param>
        /// <param name="convergenceTolerance">The convergance tolerance</param>
        /// <param name="maximumIterations">The maximum unumber of iterations</param>
        /// <returns>The minimum point</returns>
        public static MinimizationResult Minimum(IMultiObjectiveFunction objectiveFunction, DoubleVector initialGuess, DoubleVector initalPertubation, double convergenceTolerance = 1e-8, int maximumIterations = 1000)
        {
            // confirm that we are in a position to commence
            if (objectiveFunction == null)
                throw new ArgumentNullException(nameof(objectiveFunction), "ObjectiveFunction must be set to a valid ObjectiveFunctionDelegate");
            if (initialGuess == null)
                throw new ArgumentNullException(nameof(initialGuess), "initialGuess must be initialized");
            if (initialGuess == null)
                throw new ArgumentNullException(nameof(initalPertubation), "initalPertubation must be initialized, if unknown use overloaded version of FindMinimum()");
            SimplexConstant[] simplexConstants = SimplexConstant.CreateSimplexConstantsFromVectors(initialGuess, initalPertubation);
            // create the initial simplex
            int numDimensions = simplexConstants.Length;
            int numVertices = numDimensions + 1;
            var vertices = InitializeVertices(simplexConstants);
            int evaluationCount = 0;
            ExitCondition exitCondition;
            var errorValues = InitializeErrorValues(vertices, objectiveFunction);
            // iterate until we converge, or complete our permitted number of iterations
            while (true)
            {
                var errorProfile = EvaluateSimplex(errorValues);
                // see if the range in point heights is small enough to exit
                if (HasConverged(convergenceTolerance, errorProfile, errorValues))
                {
                    exitCondition = ExitCondition.Converged;
                    break;
                }
                // attempt a reflection of the simplex
                double reflectionPointValue = TryToScaleSimplex(-1.0, ref errorProfile, vertices, errorValues, objectiveFunction);
                ++evaluationCount;
                if (reflectionPointValue <= errorValues[errorProfile.LowestIndex])
                {
                    // it's better than the best point, so attempt an expansion of the simplex
                    TryToScaleSimplex(2.0, ref errorProfile, vertices, errorValues, objectiveFunction);
                    ++evaluationCount;
                }
                else if (reflectionPointValue >= errorValues[errorProfile.NextHighestIndex])
                {
                    // it would be worse than the second best point, so attempt a contraction to look
                    // for an intermediate point
                    double currentWorst = errorValues[errorProfile.HighestIndex];
                    double contractionPointValue = TryToScaleSimplex(0.5, ref errorProfile, vertices, errorValues, objectiveFunction);
                    ++evaluationCount;
                    if (contractionPointValue >= currentWorst)
                    {
                        // that would be even worse, so let's try to contract uniformly towards the low point;
                        // don't bother to update the error profile, we'll do it at the start of the
                        // next iteration
                        ShrinkSimplex(errorProfile, vertices, errorValues, objectiveFunction);
                        evaluationCount += numVertices; // that required one function evaluation for each vertex; keep track
                    }
                }
                // check to see if we have exceeded our alloted number of evaluations
                if (evaluationCount >= maximumIterations)
                {
                    throw new Exception($"Maximum iterations ({maximumIterations}) reached.");
                }
            }
            var regressionResult = new MinimizationResult(objectiveFunction, evaluationCount, exitCondition);
            return regressionResult;
        }

        /// <summary>
        /// Evaluate the objective function at each vertex to create a corresponding
        /// list of error values for each vertex
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="objectiveFunction"></param>
        /// <returns></returns>
        static double[] InitializeErrorValues(DoubleVector[] vertices, IMultiObjectiveFunction objectiveFunction)
        {
            double[] errorValues = new double[vertices.Length];
            for (int i = 0; i < vertices.Length; i++)
            {
                objectiveFunction.EvaluateAt(vertices[i]);
                errorValues[i] = objectiveFunction.Value;
            }
            return errorValues;
        }

        /// <summary>
        /// Check whether the points in the error profile have so little range that we
        /// consider ourselves to have converged
        /// </summary>
        /// <param name="convergenceTolerance"></param>
        /// <param name="errorProfile"></param>
        /// <param name="errorValues"></param>
        /// <returns></returns>
        static bool HasConverged(double convergenceTolerance, ErrorProfile errorProfile, double[] errorValues)
        {
            double range = 2 * Math.Abs(errorValues[errorProfile.HighestIndex] - errorValues[errorProfile.LowestIndex]) /
                           (Math.Abs(errorValues[errorProfile.HighestIndex]) + Math.Abs(errorValues[errorProfile.LowestIndex]) + JITTER);
            return range < convergenceTolerance;
        }

        /// <summary>
        /// Examine all error values to determine the ErrorProfile
        /// </summary>
        /// <param name="errorValues"></param>
        /// <returns></returns>
        static ErrorProfile EvaluateSimplex(double[] errorValues)
        {
            ErrorProfile errorProfile = new ErrorProfile();
            if (errorValues[0] > errorValues[1])
            {
                errorProfile.HighestIndex = 0;
                errorProfile.NextHighestIndex = 1;
            }
            else
            {
                errorProfile.HighestIndex = 1;
                errorProfile.NextHighestIndex = 0;
            }
            for (int index = 0; index < errorValues.Length; index++)
            {
                double errorValue = errorValues[index];
                if (errorValue <= errorValues[errorProfile.LowestIndex])
                {
                    errorProfile.LowestIndex = index;
                }
                if (errorValue > errorValues[errorProfile.HighestIndex])
                {
                    errorProfile.NextHighestIndex = errorProfile.HighestIndex; // downgrade the current highest to next highest
                    errorProfile.HighestIndex = index;
                }
                else if (errorValue > errorValues[errorProfile.NextHighestIndex] && index != errorProfile.HighestIndex)
                {
                    errorProfile.NextHighestIndex = index;
                }
            }
            return errorProfile;
        }

        /// <summary>
        /// Construct an initial simplex, given starting guesses for the constants, and
        /// initial step sizes for each dimension
        /// </summary>
        /// <param name="simplexConstants"></param>
        /// <returns></returns>
        static DoubleVector[] InitializeVertices(SimplexConstant[] simplexConstants)
        {
            int numDimensions = simplexConstants.Length;
            var vertices = new DoubleVector[numDimensions + 1];
            // define one point of the simplex as the given initial guesses
            var p0 = new DoubleVector(numDimensions);
            for (int i = 0; i < numDimensions; i++)
            {
                p0[i] = simplexConstants[i].Value;
            }
            // now fill in the vertices, creating the additional points as:
            // P(i) = P(0) + Scale(i) * UnitVector(i)
            vertices[0] = p0;
            for (int i = 0; i < numDimensions; i++)
            {
                double scale = simplexConstants[i].InitialPerturbation;
                var unitVector = new DoubleVector(numDimensions) {[i] = 1};
                unitVector.Multiply(scale);
                vertices[i + 1] = p0.Add(unitVector);
            }
            return vertices;
        }

        /// <summary>
        /// Test a scaling operation of the high point, and replace it if it is an improvement
        /// </summary>
        /// <param name="scaleFactor"></param>
        /// <param name="errorProfile"></param>
        /// <param name="vertices"></param>
        /// <param name="errorValues"></param>
        /// <param name="objectiveFunction"></param>
        /// <returns></returns>
        static double TryToScaleSimplex(double scaleFactor, ref ErrorProfile errorProfile, DoubleVector[] vertices,
            double[] errorValues, IMultiObjectiveFunction objectiveFunction)
        {
            // find the centroid through which we will reflect
            var centroid = ComputeCentroid(vertices, errorProfile);
            // define the vector from the centroid to the high point
            var centroidToHighPoint = vertices[errorProfile.HighestIndex].Subtract(centroid);
            // scale and position the vector to determine the new trial point
            centroidToHighPoint.Multiply(scaleFactor);
            DoubleVector newPoint = centroidToHighPoint.Add(centroid);
            // evaluate the new point
            objectiveFunction.EvaluateAt(newPoint);
            double newErrorValue = objectiveFunction.Value;
            // if it's better, replace the old high point
            if (newErrorValue < errorValues[errorProfile.HighestIndex])
            {
                vertices[errorProfile.HighestIndex] = newPoint;
                errorValues[errorProfile.HighestIndex] = newErrorValue;
            }
            return newErrorValue;
        }

        /// <summary>
        /// Contract the simplex uniformly around the lowest point
        /// </summary>
        /// <param name="errorProfile"></param>
        /// <param name="vertices"></param>
        /// <param name="errorValues"></param>
        /// <param name="objectiveFunction"></param>
        static void ShrinkSimplex(ErrorProfile errorProfile, DoubleVector[] vertices, double[] errorValues,
            IMultiObjectiveFunction objectiveFunction)
        {
            var lowestVertex = vertices[errorProfile.LowestIndex];
            for (int i = 0; i < vertices.Length; i++)
            {
                if (i != errorProfile.LowestIndex)
                {
                    vertices[i].Add(lowestVertex);
                    vertices[i].Multiply(0.5);
                    objectiveFunction.EvaluateAt(vertices[i]);
                    errorValues[i] = objectiveFunction.Value;
                }
            }
        }

        /// <summary>
        /// Compute the centroid of all points except the worst
        /// </summary>
        /// <param name="vertices"></param>
        /// <param name="errorProfile"></param>
        /// <returns></returns>
        static DoubleVector ComputeCentroid(DoubleVector[] vertices, ErrorProfile errorProfile)
        {
            int numVertices = vertices.Length;
            // find the centroid of all points except the worst one
            var centroid = new DoubleVector(numVertices - 1);
            for (int i = 0; i < numVertices; i++)
            {
                if (i != errorProfile.HighestIndex)
                {
                    centroid = centroid.Add(vertices[i]);
                }
            }
            centroid.Multiply(1.0d / (numVertices - 1));
            return centroid;
        }

        sealed class SimplexConstant
        {
            private SimplexConstant(double value, double initialPerturbation)
            {
                Value = value;
                InitialPerturbation = initialPerturbation;
            }

            /// <summary>
            /// The value of the constant
            /// </summary>
            public double Value { get; }

            // The size of the initial perturbation
            public double InitialPerturbation { get; }
       
            public static SimplexConstant[] CreateSimplexConstantsFromVectors(DoubleVector initialGuess, DoubleVector initialPertubation)
            {
                var constants = new SimplexConstant[initialGuess.Count];
                for (int i = 0; i < constants.Length; i++)
                {
                    constants[i] = new SimplexConstant(initialGuess[i], initialPertubation[i]);
                }
                return constants;
            }
        }

        sealed class ErrorProfile
        {
            public int HighestIndex { get; set; }
            public int NextHighestIndex { get; set; }
            public int LowestIndex { get; set; }
        }
    }
}
