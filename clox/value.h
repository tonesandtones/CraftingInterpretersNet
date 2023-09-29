//
// Created by Anthony on 28/09/2023.
//

#ifndef clox_value_h
#define clox_value_h

#include "common.h"

typedef double Value;

typedef struct {
    int capacity;
    int count;
    Value* values;
} ValueArray;

//Initialise members of a ValueArray to valid initial state
void initValueArray(ValueArray* array);

//Append a new value to a ValueArray
void writeValueArray(ValueArray* array, Value value);

//Free the memory allocated to a Value Array and reinitialise
void freeValueArray(ValueArray* array);

//print the literal value of a Value
void printValue(Value value);

#endif //clox_value_h
