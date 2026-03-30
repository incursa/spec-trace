# Integer Calculator Source Notes

This note captures the upstream request for the integer calculator example.
It is an input artifact for `Upstream Refs`, not canonical requirement text.

## API Shape

- The example uses one calculator class.
- The class exposes five methods: add, subtract, multiply, divide, and modulus.
- Each method accepts two signed 32-bit integer operands and returns a signed 32-bit integer result.

## Addition

- Addition returns the mathematical sum.
- Operand order does not change the result.
- Results greater than `2147483647` are rejected as overflow.
- Results less than `-2147483648` are rejected as underflow.

## Subtraction

- Subtraction returns the first operand minus the second operand.
- Operand order matters.
- Results greater than `2147483647` are rejected as overflow.
- Results less than `-2147483648` are rejected as underflow.

## Multiplication

- Multiplication returns the mathematical product.
- Operand order does not change the result.
- Results greater than `2147483647` are rejected as overflow.
- Results less than `-2147483648` are rejected as underflow.

## Division

- Division uses integer results.
- A zero divisor is rejected.
- When the mathematical quotient is not an integer, the result truncates toward zero.
- A negative divisor is allowed when it is not zero.
- The quotient is positive when the dividend and divisor have the same sign and negative when they have different signs.

## Modulus

- Modulus uses integer results.
- A zero divisor is rejected.
- The remainder follows truncation-toward-zero division semantics.

## Integer Range And Overflow

- The example uses the signed 32-bit integer range from `-2147483648` to `2147483647`.
- Boundary operand values are valid when the specified result is still representable.
- Operations whose required result is outside the signed 32-bit range are rejected as overflow.
- Results below `-2147483648` are treated as underflow and are rejected the same way as results above `2147483647`.
