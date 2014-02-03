.info
	.source "PapyrusDotNet-Generated.psc"
	.modifyTime 1391440704
	.compileTime 1391440704
	.user "Karlj"
	.computer "CD197"
.endInfo
.userFlagsRef
	.flag conditional 1
	.flag hidden 0
.endUserFlagsRef
.objectTable
	.object Example1_ListExample ObjectReference
		.userFlags 0
		.docString ""
		.autoState
		.variableTable
			.variable ::listOfIntegers DotNetList_Int
				.userFlags 0
				.initialValue None
			.endVariable
		.endVariableTable
		.propertyTable
			.property plistOfIntegers DotNetList_Int auto
				.userFlags 0
				.docString ""
				.autoVar ::listOfIntegers
			.endProperty
		.endPropertyTable
		.stateTable
			.state
				.function OnInit
					.userFlags 0
					.docString ""
					.return None
					.paramTable
					.endParamTable
					.localTable
						.local ::NoneVar None
						.local V_0 Int
						.local V_1 Bool
					.endLocalTable
					.code
						Assign V_0 0
						Jump _label35
					_label16:
						CallMethod Add ::listOfIntegers ::NoneVar V_0
						IAdd V_0 V_0 1
					_label35:
						CompareLT V_1 V_0 10
						JumpF V_1 _label16
						Return None
					.endCode
				.endFunction
			.endState
		.endStateTable
	.endObject
.endObjectTable