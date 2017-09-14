Example 1 - Training On Start

Brief
-----

In this example we are showing how to make an intelligent car in the unity editor that have learned
previusly how to avoid a wall. With the MonoNeuralNet script we create a neural net that will listen 
the car left and right sensors and returns the appropiate turn direction. 

The neural net will listen the sensors by two input neurons, one for each sensor, we need to 
scale the distance values of each sensor in a 0-1 range because the neural net only recognizes that values.
The turn direction will be returned by the neural net by a single output neuron also in a 0-1 range.

We train the network such a way that the return value will be 0 if we need to turn right, 0.5 if we
need to keep the direction and 1 if we need to turn left. Depending in how much train loops we have 
specified in the MonoNeuralTrainer script, the returned value of the neural net will be more accurrate,
but never will be exact, so we will check if the value is in a certain range, i.e. to turn left the value
needs to be greater than 0.9. Take note that we would also change the neural net to have two output neurons
but we try to show all the posibilities.

How we do that with the framework
---------------------------------

We archieve this simply with the use of the scripts provided by the framework and also by creating a car
controller script.

To make a neural network we simply drag a MonoNeuralNet script in the GameObject we prefer, in this case
we create an empty GameObject called "IA" inside our car, then we set the quantity of input and output 
neurons, and also, we set the amount of hidden layers, and how neurons per hidden layer we want, many
hidden layers with many neurons will be more accurate to the wanted response, but it will be more expensive
in terms of processing, try to adjust the quantity of hidden layers and neurons per layer in base of the
input and output neurons quantity and the quantity of different behaviours.

For this case one hidden layer with five neurons will be more than enough.

The neural net by itself its useless, to make it intelligen we need to train the neural net. Train is the
act of showing the expected results of a combination of inputs to the neural net, i.e. tell the neural net
that if the left sensor its giving 0, that means that the car its or will collide with the wall, so the neural
net needs to respond that we need to turn right. 

To train the neural net we need two scripts MonoNeuralTrainer y MonoNeuralTrainCase, the two can be added in the
same game object that contains the neural net, so the neural net to train will be automatically setted to the one
that its contained by the game object, or we can put the trainer in another game object and then manually
drag the neural net to the neuralNet property of the trainer (also this type of logic happens with the neuraltrainer 
and the neural train cases, but can be combined because the cases can be multiple). So in this case we have all in 
the same GameObject "IA" inside the car. Then to make the neural net act like we want, simply we drag one trainer 
to the net and four train cases. We also set the trainLoops of the trainer to 10000, the train loops specify how times 
we make the neural net learn all the train cases we have in the game object, more higher this number, more accurate
will be the results. And then we set all the four cases, see in each case we specify one of the possible results, see
in this case can happen four things, one can be that the sensors returns 1 and 1, that means that anything its near
the car and we need to keep stright our way, that means that the neural net should return 0.5, another can be that the 
sensors returns 0 and 0, that means that the car its being collided by the two sides and cant turn (in this case we do 
nothing, but we can implement another network, or also use the same to not only turn left or right, also we can make 
acceleerate forward or backward), another one can be 1 and 0, that we are totally hitting the left side and we need to 
turn right so the neural net needs to return 1 and finally 0 and 1 that its the backwards of the previous case. 

Finally see that we smooth the return value using the range method of math utils, but you can comment that code and
uncomment the code commented down that code and see the difference.

Its time to see the scripts in the "IA" GameObject on the Car and see how easy is create a neural network with our
framework.

Thanks To Read, Dont Forget to Rate Us in the asset store, and when you are ready proceed with the next example to
see all the amazing features of our framework.

Nicolas Borromeo - Framework Lead Programmer (nicolas.borromeo@gmail.com)