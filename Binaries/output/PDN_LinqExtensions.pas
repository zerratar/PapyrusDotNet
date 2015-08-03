.info
	.source "PapyrusDotNet-Generated.psc"
	.modifyTime 1438614238
	.compileTime 1438614238
	.user "Karlj"
	.computer "CD197"
.endInfo
.userFlagsRef
	.flag conditional 1
	.flag hidden 0
.endUserFlagsRef
.objectTable
.object PDN_LinqExtensions
		.userFlags 0
		.docString ""
		.autoState
		.variableTable
		.endVariableTable
		.propertyTable
		.endPropertyTable
		.stateTable
			.state
				.function LastOrDefault static
					.userFlags 0
					.docString ""
					.return T
					.paramTable
						.param source T[]
						.param predicate Func`2
					.endParamTable
					.localTable
						.local V_0 Int
						.local V_1 T
						.local V_2 Bool
						.local V_3 T
						.local ::temp2 !1
						.local ::temp1 T
						.local ::temp0 Int
					.endLocalTable
					.code
						ArrayLength ::temp0 source
						Assign V_0 ::temp0
						Jump _label44
					_label7:
						ArrayGetElement ::temp1 source V_0
						CallMethod Invoke predicate ::temp2 ::temp1
						Cast V_2 ::temp2
						CompareEQ V_2 ::temp2 0
						JumpF V_2 _label39
						ArrayGetElement V_1 source V_0
						Jump _label64
					_label39:
						ISubtract V_0 V_0 1
					_label44:
						CompareGT V_2 V_0 0
						JumpF V_2 _label7
						Assign V_1 V_3
						Jump _label64
					_label64:
						Return V_1
					.endCode
				.endFunction
				.function FirstOrDefault static
					.userFlags 0
					.docString ""
					.return T
					.paramTable
						.param source T[]
						.param predicate Func`2
					.endParamTable
					.localTable
						.local V_0 T
						.local V_1 T
						.local V_2 T[]
						.local V_3 Int
						.local V_4 Bool
						.local V_5 T
						.local ::temp1 Int
						.local ::temp0 !1
					.endLocalTable
					.code
						Assign V_2 source
						Assign V_3 0
						Jump _label43
					_label8:
						ArrayGetElement V_0 V_2 V_3
						CallMethod Invoke predicate ::temp0 V_0
						Cast V_4 ::temp0
						CompareEQ V_4 ::temp0 0
						JumpF V_4 _label38
						Assign V_1 V_0
					_label38:
						IAdd V_3 V_3 1
					_label43:
						ArrayLength ::temp1 V_2
						CompareLT V_4 V_3 ::temp1
						JumpF V_4 _label8
						Assign V_1 V_5
						Jump _label68
					_label68:
						Return V_1
					.endCode
				.endFunction
			.endState
		.endStateTable
	.endObject
.endObjectTable