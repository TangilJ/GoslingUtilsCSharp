using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Bot.Gosling.Routines;

namespace Bot.Gosling
{
    /// <summary>
    /// This class is for strategic tools.
    /// </summary>
    public static class Tools
    {
        /// <summary>
        /// FindHits takes a dict of (left, right) target pairs and finds routines that could hit the ball between those
        /// target pairs.<br/>
        /// FindHits is only meant for routines that require a defined intercept time/place in the future.<br/>
        /// FindHits should not be called more than once in a given tick, as it has the potential to use an entire tick
        /// to calculate.<br/>
        /// </summary>
        /// <remarks>
        /// Porting note: The above note that this method shouldn't be called frequently probably does not apply to C#
        /// as it's much faster than Python.
        /// </remarks>
        /// <example>
        /// Example usage:
        /// <code>
        /// var targets = new Dictionary&lt;string, (Vector3, Vector3)&gt;
        /// {
        ///     {"goal", (opponentLeftPost, opponentRightPost)},
        ///     {"anywhereButMyNet", (myRightPost, myLeftPost)}
        /// }
        /// var hits = FindHits(agent, targets);
        /// hits == Dictionary&lt;string, List&lt;IRoutine&gt;&gt;
        /// {
        ///     { "goal", List&lt;IRoutine&gt; { a ton of jump and aerial routines, in order from soonest to latest } },
        ///     { "anywhereButMyNet", List&lt;IRoutine&gt; { more routines and stuff } }
        /// }
        /// </code>
        /// </example>
        public static Dictionary<string, List<IRoutine>> FindHits(
            GoslingAgent agent,
            Dictionary<string, (Vector3, Vector3)> targets
        )
        {
            var hits = targets.ToDictionary(target => target.Key, _ => new List<IRoutine>());
            var @struct = agent.GetBallPrediction();
            // Porting note: the name for the above variable in the original code is `struct`, which is why we need to
            // use @ (verbatim identifier).

            // Begin looking at slices 0.25s into the future
            // The number of slices
            var i = 15;
            while (i < @struct.Length)
            {
                // Gather some data about the slice
                var interceptTime = @struct.Slices[i].GameSeconds;
                var timeRemaining = interceptTime - agent.Time;
                if (timeRemaining > 0)
                {
                    var ballLocation = @struct.Slices[i].Physics.Location;
                    var ballVelocity = @struct.Slices[i].Physics.Velocity.Length();

                    if (Math.Abs(ballLocation.X) > 5250)
                        break; // Abandon search if ball is scored at/after this point

                    // Determine the next slice we will look at, based on ball velocity (slower ball needs fewer slices)
                    i += 15 - (int) Utils.Cap((int) (ballVelocity / 150), 0, 13);

                    var carToBall = ballLocation - agent.Me.Location;
                    var direction = Vector3.Normalize(carToBall);
                    var distance = carToBall.Length();

                    // How far the car must turn in order to face the ball, for forward and reverse
                    var forwardAngle = direction.Angle(agent.Me.Forward);
                    var backwardAngle = Math.PI - forwardAngle;

                    // Accounting for the average time it takes to turn and face the ball
                    // Backward is slightly longer as typically the car is moving forward and takes time to slow down
                    var forwardTime = timeRemaining - (forwardAngle * 0.318f);
                    var backwardTime = timeRemaining - (backwardAngle * 0.418f);

                    // If the car only had to drive in a straight line, we ensure it has enough time to reach the
                    // ball (a few assumptions are made)
                    var forwardFlag =
                        forwardTime > 0 && (distance * 1.025f / forwardTime) < (agent.Me.Boost > distance / 100
                            ? 2299
                            : Math.Max(1400, 0.8 * agent.Me.Velocity.Flatten().Length()));

                    var backwardFlag = distance < 1500 && backwardTime > 0 && (distance * 1.05 / backwardTime) < 1200;

                    // Provided everything checks out, we begin to look at the target pairs

                    // Porting note: we inverted the if condition here in order to reduce nesting.
                    // The logic behaves the same.
                    if (!forwardFlag && !backwardFlag)
                        continue;

                    foreach (var pair in targets)
                    {
                        // First we correct the target coordinates to account for the ball's radius
                        // If fits == true, the ball can be scored between the target coordinates
                        var (left, right, fits) =
                            Utils.PostCorrection(ballLocation, pair.Value.Item1, pair.Value.Item2);

                        // Porting note: we inverted the if condition here in order to reduce nesting.
                        if (!fits)
                            continue;

                        // Now we find the easiest direction to hit the ball in order to land it between the
                        // target points
                        var leftVector = Vector3.Normalize(left - ballLocation);
                        var rightVector = Vector3.Normalize(right - ballLocation);
                        var bestShotVector = Vector3.Clamp(direction, leftVector, rightVector);

                        // Check to make sure our approach is inside the field
                        // The slope represents how close the car is to the chosen vector, higher = better
                        // A slope of 1.0 would mean the car is 45 degrees off
                        var slope = Utils.FindSlope(bestShotVector.Flatten(), carToBall.Flatten());
                        if (forwardFlag)
                        {
                            var ballInField = Utils.InField(ballLocation, 200);
                            var carInField = Utils.InField(agent.Me.Location, 100);
                            if ((ballLocation.Z <= 300 || !ballInField && !carInField) && slope > 0)
                                hits[pair.Key].Add(new JumpShot(ballLocation, interceptTime, bestShotVector, slope));

                            var ballCap = Utils.Cap(ballLocation.Z - 400, 100, 2000) * 0.1f;
                            var ballBoostCondition = ballCap < agent.Me.Boost;

                            // Porting note: we inverted the if condition here in order to reduce nesting.
                            if (!(ballLocation.Z > 325) || !(slope > 1) || !ballBoostCondition)
                                continue;

                            if ((carToBall / forwardTime - agent.Me.Velocity).Abs().Length() - 300 < 400 * forwardTime)
                                hits[pair.Key].Add(new AerialShot(ballLocation, interceptTime, bestShotVector, slope));
                        }
                        else if (ballLocation.Z <= 280 && slope > 0.25)
                            hits[pair.Key].Add(new JumpShot(ballLocation, interceptTime, bestShotVector, slope, -1));
                    }
                }
                else
                    i++;
            }

            return hits;
        }
    }
}