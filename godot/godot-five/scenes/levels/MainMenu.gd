extends Control

func OnPathfindingTestMenu_pressed():
	get_tree().change_scene_to_file("res://scenes/levels/testing/NavigationPlayground.tscn")


func _on_main_level_buttonpressed():
	get_tree().change_scene_to_file("res://scenes/levels/Orchardlevel.tscn")
