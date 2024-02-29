using FluentValidation;

namespace Parametrization.Validation;

public sealed class SimplePortParametersValidator : AbstractValidator<SimplePortParameters>
{
	public SimplePortParametersValidator()
	{
		RuleFor(param => param.CutterRadius).GreaterThanOrEqualTo(0).
			WithMessage("Cutter radius must be greater or equal to 0.");
		RuleFor(param => param.RightWebWidth).GreaterThanOrEqualTo(0).
			WithMessage("Right web width must be greater or equal to 0.");
		RuleFor(param => param.LeftWebWidth).GreaterThanOrEqualTo(0).
			WithMessage("Left web width must be greater or equal to 0.");
		RuleFor(param => param.CoreOffset).GreaterThanOrEqualTo(0).
			WithMessage("Core offset must be greater or equal to 0.");
		RuleFor(param => param.CornerOffset).GreaterThan(param => param.CoreOffset).
			WithMessage("Corner offset must be greater than core offset.");
		RuleFor(param => param.WeldChamberOffset).GreaterThan(param => param.CornerOffset).
			WithMessage("Weld chamber offset must be greater than corner offset.");
	}
}