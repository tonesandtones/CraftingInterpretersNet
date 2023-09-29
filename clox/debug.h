//
// Created by Anthony on 22/09/2023.
//

#ifndef clox_debug_h
#define clox_debug_h

#include "chunk.h"

//Walk the code array of a chunk, disassemble and print each instruction in the code array
void disassembleChunk(Chunk* chunk, const char* name);

//Disassemble and print a instruction at the given offset. Return the offset of the next instruction
int disassembleInstruction(Chunk* chunk, int offset);

#endif //CLOX_DEBUG_H
