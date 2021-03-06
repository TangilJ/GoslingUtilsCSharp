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

So far, this port is **unfinished**. While the most important things have been implemented, the routines have not yet
been touched and some utility methods need to be completed.

If you would like to help complete the port, please feel free to make a pull request. If you have any questions, don't
hesitate to ask via the Issues tab on this repo or on Discord.

Note that this port is based on commit
[733b6b0](https://github.com/ddthj/GoslingUtils/tree/733b6b05bc9cab8da596d6ed324fbfbf179100a0) of the original
repository.

## Porting notes

This C# version is mostly a 1-to-1 port of the original Python implementation. However, there are differences where it
makes sense to have them, such as using interfaces when the original doesn't use them. Such differences are noted in the
source code with explanations as to why the port strays from the original.

Typically, the usage of the C# port should be similar to the usage of the original Python implementation, but make sure
to look at the C# source code for comments in case you're expecting something different with certain code.

Comments are the same as the Python version. Comments explaining differences between the original and the port are
prefixed with "Porting note" so that it's clear that they're not part of the original comments. Additional documentation
comments are also prefixed with "Porting note".

## To do

There are a number of things that are left to be done for this port:

### Must

* Complete all the implementations for the routines
* Complete the method in Tools
* Complete the methods in Utils

### Should

* Write down the differences between the port and the original
* Create a port of Gosling using these ported utilities

### Could

* Possibly create a more idiomatic version of this port with intentionally different design decisions to fit C# better

## Requirements

This project was tested on .NET Framework v4.6.1. Your mileage may vary on newer versions like .NET 5.