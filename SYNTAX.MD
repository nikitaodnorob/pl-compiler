# Syntax rules of my language

## General rules
* After any statement goes semicolon `;`
* It's a language with static typing, all standard .NET types are supported
* Accepted names for variables, functions and other entities are words which consist of latin letters, digits and underline symbol `_`,
but first symbol isn't a digit
* Function `Main` isn't required, but creating function with `Main` name is disallowed
* Comments aren't supported now

## Operators
* Output operator: `print <expression>;`
* Ariphmetic operators: `+ - * / % ()`

## Variables define
```
int a, b = 5;
double c = 3.14;
(int d, int e) = (10, 20);
int varScope = 5;
{
    double varScope = 7;
}
```

## Variables assign
```
a = 1;
(a, b) = (1, 2);
```

## Functions
```
int first(int a, int b) {
    return a;
}
void print10() {
    print 10;
}

print first(1,2);
print10();
```

## Cycles
```
print "Numbers from 1 to 5";
for int i in 1..5 print i;

print "Number 10 at five times";
loop (5) print 10;
```

## Arrays
```
int[] array = int {1,2,3,4,5};
print array;
print array.indices;

print array[0];

array.swapByIndex(0, 1);
print array;

int a, b, c, d, e;
(a, b, c, d, e) = array;
```

## Using .NET classes
```
netusing System.Threading;

Thread.Sleep(1000);
print System.Math.Sin(30 * Math.PI / 180);
```

## Perfomance count
```
double sum = 0;
Performance.start();

for int i in 1..10000
	for int j in 1..10000
		sum = sum + 1.0/(i+j);

Performance.stop();
print sum;
print Performance.milliseconds;
```
