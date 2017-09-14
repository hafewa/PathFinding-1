Example 2 - Training On Wall Hit

Brief
-----

In this example we gonna see how to train our network in a way that we dont show the neural net all the
train cases at the start. We gonna train our network dinamically in every error. We gonna detect when
the neural net make us hit a wall, an in that case, we gonna tell the net to turn the opposite side.

Please make sure you have read and understand the concepts of tutorial 1, because we gonna need them to
understand this tutorial. Also to see this method of learning please be patient and see how the net lerans
when to turn after a few collisions.

How we do that with the framework
---------------------------------

We have taken the previous example so you should be familiar with it. You can appreciate that in this case
the IA gameObject inside the Car gameObject (from now we gonna call the gameObject as GO), has less scripts
attached to it, we only have the MonoNeuralNet script this time. In the previous example we has trained the
neural net through some trainer scripts, but in this case we gonna manually train the network using the
Train method of the MonoNeuralNet script. 

If we see the code in the CarMovement2 script, we see almost thevsame code of the previous example, but the 
difference is a new block when you see, we gonna check for thevneural net input values, that means the sensor 
return values. See that we check the value of the input to see if reaches 0, that means that the car has hit 
(remember that the range method returns 0 if the given valueis less or equals the minimalValue). 
In that case we gonna call the Train Method several times to adapat the neural net to the new response that has 
to return the next time and also we relocates the car in the original position. If we see the train method call, 
we see that receives two values, the first is the input values that the neural net gonna receive, but are incorrect,
and the second value has a hadcoded value that represents the ideal value that the neural net needs to return in 
that case. So the train method trains the neural net to respond the ideal value with that particular input that we 
have determined that its incorrect. So we do that with both sides.

Also you can see the difference in the turn logic and the training to see that that also affects the neural net,
so make sure dont use the same neural net with objects with difference capabilitis, in this case we refeer to cars
that cant turn faster, or turns faster. Remember that this can be applied to anything you want, not only cars.

Thanks To Read, Dont Forget to Rate Us in the asset store, and when you are ready proceed with the next example to
see all the amazing features of our framework.

Nicolas Borromeo - Framework Lead Programmer (nicolas.borromeo@gmail.com)