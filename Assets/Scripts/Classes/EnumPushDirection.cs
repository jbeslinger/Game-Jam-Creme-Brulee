/// <summary>
/// Describes a direction to move on the Gameboard.
/// Direct is a special case that should only apply to the piece that the
/// palyer has grabbed and is placing somewhere.
/// </summary>
public enum PushDirection { UP = 0, RIGHT = 1, DOWN = 2, LEFT = 3, DIRECT = 99 };
