extends CharacterBody3D

var speed = 2
var acceleration = 10;

@onready var nav : NavigationAgent3D = $NavigationAgent3D

func _physics_process(delta):
	var direction = Vector3()
	
	nav.target_position = global_position + Vector3(20,0,20)
	direction = nav.get_next_path_position() - global_position
	direction = direction.normalized()
	
	velocity = velocity.lerp(direction* speed, acceleration * delta)
	
	move_and_slide()
