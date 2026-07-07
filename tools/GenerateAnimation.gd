@tool
extends EditorScript

func _run():
	var nodes = EditorInterface.get_selection().get_selected_nodes()
	var sprite = nodes[0] as Sprite2D
	var anim_player = nodes[1] as AnimationPlayer

	var anim_name = "relaxed_loop"
	var fps = 60
	var total_frames = 180

	var anim = Animation.new()
	anim.fps = fps
	anim.length = total_frames / float(fps)
	anim.loop = true

	var track = anim.add_track(Animation.TYPE_VALUE)
	anim.track_set_path(track, String(sprite.get_path()) + ":texture")

	for i in range(total_frames):
		var path = "res://Tsukiyuki Miyako/image/shop/shop%03d.png" % i
		var tex = load(path)
		anim.track_insert_key(track, i / float(fps), tex)

	anim_player.get_animation_library("").add_animation(anim_name, anim)
	print("轨道生成完成！")
