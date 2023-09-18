//
// Created by Anthony on 18/09/2023.
//

#ifndef clox_chunk_h
#define clox_chunk_h

#include "common.h"

typedef enum {
    OP_RETURN,
} OpCode;

typedef struct {
    uint8_t* code;
} Chunk;

#endif