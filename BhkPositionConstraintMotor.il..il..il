.class public auto ansi BhkPositionConstraintMotor
	extends [netstandard]System.Object
{
	// Fields
	.field public float32 MinForce
	.field public float32 MaxForce
	.field public float32 Tau
	.field public float32 Damping
	.field public float32 ProportionalRecoveryVelocity
	.field public float32 ConstantRecoveryVelocity
	.field public bool MotorEnabled

	// Methods
	.method public specialname rtspecialname 
		instance void .ctor (
			class [netstandard]System.IO.BinaryReader ''
		) cil managed 
	{
		.param [1] = nullref
		// Method begins at RVA 0x2128
		// Code size 13 (0xd)
		.maxstack 8

		// 	MotorEnabled = P_0.ReadBoolean();
		IL_0000: ldarg.0
		IL_0001: ldarg.1
		IL_0002: callvirt instance bool [netstandard]System.IO.BinaryReader::ReadBoolean()
		IL_0007: stfld bool BhkPositionConstraintMotor::MotorEnabled
		// (no C# code)
		IL_000c: ret
	} // end of method BhkPositionConstraintMotor::.ctor

} // end of class BhkPositionConstraintMotor
