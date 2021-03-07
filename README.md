[<img src="https://img.shields.io/github/workflow/status/TheBlocks/GoslingUtilsCSharp/.NET">](https://github.com/TheBlocks/GoslingUtilsCSharp/actions)
[<img src="https://img.shields.io/github/license/TheBlocks/GoslingUtilsCSharp">](https://github.com/TheBlocks/GoslingUtilsCSharp/blob/master/LICENSE)

# GoslingUtilsCSharp

This is a C# port of GoslingUtils, which is a set of tools in Python for making RLBots. See the original
here: https://github.com/ddthj/GoslingUtils.

You should be able to follow along with the
[RLBot Start-To-Finish series](https://www.youtube.com/playlist?list=PL2MGDOTjPtl8fuoXmqxTmASW1ZtrPEXQ2) with this port,
even if the series is in Python. Just make sure to check the source code for the port in case you expect something
different.

Note that this port tries to be close to the original implementation, which is why some code may not be idiomatic C# or
done in a way you might expect from C#. Nevertheless, this should make it easier to follow along with the tutorial
series.

If you notice any bugs/issues, please feel free to create an issue via the Issues tab or make a pull request.

Note that this port is based on commit
[733b6b0](https://github.com/ddthj/GoslingUtils/tree/733b6b05bc9cab8da596d6ed324fbfbf179100a0) of the original
repository.

## Porting notes

This C# version is mostly a 1-to-1 port of the original Python implementation. However, there are differences where it's
necessary to have them, such as using interfaces when the original doesn't use them. Such differences are noted in the
source code with explanations as to why the port strays from the original.

Typically, the usage of the C# port should be similar to the usage of the original Python implementation, but make sure
to look at the C# source code for comments in case you're expecting something different with certain code.

Comments are the same as the Python version. Comments explaining differences between the original and the port are
prefixed with "Porting note" so that it's clear that they're not part of the original comments. Additional documentation
comments are also prefixed with "Porting note".

## Architectural differences

The port is very similar to the original implementation, but because of the type system there are some minor things to
be noted.

* In the original implementation, all routine classes have a `run` method but there are no interfaces or abstract
  classes. In C#, we cannot have this without dynamic typing, so we have an interface in the port called `IRoutine` with
  a `Run` method.
* Similarly, all routine classes for shots have `intercept_time` and `ball_location` fields in the Python
  implementation. In the C# port, we have an interface called `IShotRoutine` with these as properties.
* The original implementation creates its own internal data structures for the Ball and GameInfo. We don't need to do
  this because the example bot template already comes with nice to use structures. This is why you need to
  write `Ball.Physics.Location` but in the original it's `ball.location` (without the `.physics`);

## To do

There are a number of things that are left to be done for this port:

### ~~Must~~ (port is finished)

### Should

* Create a port of Gosling using these ported utilities

### Could

* Possibly create a more idiomatic version of this port with intentionally different design decisions to fit C# better

## Requirements

This project was tested on .NET Framework v4.6.1. Your mileage may vary on newer versions like .NET 5.

The project requires the RLBot.Framework package and the System.ValueTuple package from NuGet. Your IDE should be able
to automatically fetch these packages on first build, but if you run into issues try manually uninstalling and
reinstalling them. 