﻿using System;

namespace CraftingInterpretersNet.Generator;

internal partial class ExprGenerator
{
    private static readonly string GeneratedCodeAttribute = $@"[global::System.CodeDom.Compiler.GeneratedCodeAttribute(""{typeof(ExprGenerator).Assembly.GetName().Name}"", ""{typeof(ExprGenerator).Assembly.GetName().Version}"")]";
    

    private static readonly string Preamble = $"""
                                        // <auto-generated/>
                                        using System.Collections.Generic;{Environment.NewLine}
                                        namespace CraftingInterpretersNet.Generator;{Environment.NewLine}
                                        """;
}