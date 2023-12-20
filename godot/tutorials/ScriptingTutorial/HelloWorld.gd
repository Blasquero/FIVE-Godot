extends Sprite2D

var Speed = 400
var AngularSpeed = PI
# Called when the node enters the scene tree for the first time.
func _ready():
 # Replace with function body.
	pass

func _init():
	print("Hello, world!")
	
# Called every frame. 'delta' is the elapsed time since the previous frame.
func _process(delta):
	rotation+= AngularSpeed * delta;
	var velocity = Vector2.UP.rotated(rotation) * Speed
	position += velocity * delta
