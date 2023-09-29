//
// Created by Anthony on 18/09/2023.
//

#ifndef clox_chunk_h
#define clox_chunk_h

#include "common.h"
#include "value.h"

typedef enum {
    OP_CONSTANT,
    OP_ADD,
    OP_SUBTRACT,
    OP_MULTIPLY,
    OP_DIVIDE,
    OP_NEGATE,
    OP_RETURN,
} OpCode;

typedef struct {
    int count;
    int capacity;
    uint8_t* code;
    int* lines;
    ValueArray constants;
} Chunk;

//Initialise the members of a chunk to valid initial state
void initChunk(Chunk* chunk);

//Add a code instruction to the chunk, growing the chunk code array if necessary
void writeChunk(Chunk* chunk, uint8_t byte, int line);

//Free all memory allocated to the chunk and reinitialise
void freeChunk(Chunk* chunk);

//Adds a value to the chunks constant array. Returns the index into the constant array of the Value that was just added.
int addConstant(Chunk* chunk, Value value);

#endif