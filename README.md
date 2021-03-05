# GoslingUtilsCSharp

This is a C# port of GoslingUtils, which is a set of tools in Python for making RLBots. See the original
here: https://github.com/ddthj/GoslingUtils.

You should be able to follow along with the RLBot Start-To-Finish series with this port, even if the series is in
Python. Just make sure to check the source code for the port in case you expect something different.

## Porting notes

This C# version is mostly a 1-to-1 port of the original Python implementation. However, there are differences where it
makes sense to have them, such as using interfaces when the original doesn't use them. Such differences are noted in the
source code with explanations as to why the port strays from the original.

Typically, the usage of the C# port should be similar to the usage of the original Python implementation, but make sure
to look at the C# source code for comments in case you're expecting something different with certain code.

Comments are the same as the Python version. Comments explaining differences between the original and the port are
prefixed with "Porting note" so that it's clear that they're not part of the original comments. Additional documentation
comments are also prefixed with "Porting note".