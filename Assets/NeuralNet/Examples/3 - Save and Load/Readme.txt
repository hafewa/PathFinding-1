Example 3 - Save and Load

Brief
-----

In this example we gonna see how to save and load all the training of the neural net into a binary file, thia 
can be useful to make intelligence editors for developers where we can train a net and then save the changes 
to keep training later or maybe do some amazing gameplay mechanics making an intelligence evolves as part of 
the game.

Continuing the previous train on wall hit tutorial we gonna save and load when we want. For that the examples
shows buttons for save and load. You can save certain learn stage when the net its not fully trained and then
reload the level and hit the load button to see that the last saved changes to the net has been succesfully
load. If you hit the load button the first time, this exampes comes with a example file of the fully trained
net.

You can modify the file property in the CarMovement3 script to save the neural net in another place

How we do that with the framework
---------------------------------

This can be simply archieved calling the extension method(*) Read and Persist of MonoNeuralNet and passing the
path of the file to put the info. Remember that the file its in binary format, so if you open with a text
editor, it wont show any info in a familiar format, so dont manually touch that file or undesired errors
will happen. 


(*)An extension method is a static method that automatically its added as an instance method of all objects that
are of the type of the parameter in the method that has the this keyword. So if you try to use Read and Persist
from your program and dont appear, dont forget to add an using to the TongFramework.Persistence library, that
will add the method.

More info about extension methods can be found in this link:
http://msdn.microsoft.com/en-us/library/bb383977.aspx

Thanks To Read, Dont Forget to Rate Us in the asset store, and make amazing things with Neural Nets! 

Nicolas Borromeo - Framework Lead Programmer (nicolas.borromeo@gmail.com)