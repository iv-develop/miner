extends CharacterBody2D


var SPEED = 80.0
var GAIN_SPEED = 3.2
var LOSS_SPEED = 2.0

var DIGGER_ROTATION_SPEED = 2.5
var RECOIL_FORCE = 100.0
var DIGGER_CD = 0.5

@onready var digger = $Digger;
@onready var digger_cd = $DiggerCD;
@onready var map = $"../Map"
@onready var oremap = $"../OreMap"
func _ready():
	ORES.Print()
	oremap.tile_set = ORES.GenerateOreTileset()

var chunk_size = 16;
var tile_size = 16;
var spawn_radius = 1;
var spawned_chunks = {Vector2i(0, 0): null}

func get_chunk_pos():
	return Vector2i(floor(global_position / (chunk_size * tile_size)))

func spawn_chunk(p: Vector2i):
	if spawned_chunks.has(p): return
	spawned_chunks[p] = null
	var start = chunk_size * p
	var tiles : Array[Vector2i] = []
	for x in range(chunk_size):
		for y in range(chunk_size):
			tiles.append(start + Vector2i(x, y))
			var ores = ORES.TickTile();
			for ore in ores:
				var id = ore[0]
				var variants = ore[1]
				var offset = ore[2]
				oremap.set_cell(start + Vector2i(x, y) + offset, id, Vector2i(randi_range(0, variants), 0))
	map.set_cells_terrain_connect(tiles, 0, 0)

func spawn_chunks_around():
	for x in range(-1 * spawn_radius, spawn_radius + 1):
		for y in range(-1 * spawn_radius, spawn_radius + 1):
			spawn_chunk(current_chunk_position + Vector2i(x, y))

var current_chunk_position = null
func _process(_dt: float) -> void:
	var new_chunk = get_chunk_pos()
	if current_chunk_position != new_chunk:
		current_chunk_position = new_chunk
		spawn_chunks_around()
	
		
		

func _physics_process(delta: float) -> void:
	SPEED = 80.0
	GAIN_SPEED = 3.2
	DIGGER_ROTATION_SPEED = 10.0;
	RECOIL_FORCE = 45.0
	LOSS_SPEED = 1.75
	DIGGER_CD = 0.25
	var dir := Input.get_vector("left", "right", "up", "down");
	if digger.is_colliding() and digger_cd.is_stopped():
		var tile_pos = map.local_to_map(digger.get_collision_point(0) - digger.get_collision_normal(0))
		if !map.get_cell_tile_data(tile_pos):
			var d_dir = digger.get_collision_point(0) - global_position
			var fix = Vector2(sign(d_dir.x) * 1, 0)
			if d_dir.x > d_dir.y:
				fix = Vector2(0, sign(d_dir.y) * -1)
			tile_pos = map.local_to_map(digger.get_collision_point(0) - digger.get_collision_normal(0) + fix)
		map.set_cells_terrain_connect([tile_pos], 0, -1)
		oremap.erase_cell(tile_pos)
		digger_cd.start(DIGGER_CD)
		var digger_angle = digger.rotation;
		velocity = digger.get_collision_normal(0) * RECOIL_FORCE # ( Vector2(-1.0, 0.0) * RECOIL_FORCE ).rotated(digger_angle)
	
	if dir:
		var target = dir.normalized() * SPEED;
		var target_digger_angle = atan2(dir.y, dir.x);
		velocity = Vector2(move_toward(velocity.x, target.x, GAIN_SPEED), move_toward(velocity.y, target.y, GAIN_SPEED));
		digger.rotation = lerp_angle(digger.rotation, target_digger_angle, delta * DIGGER_ROTATION_SPEED);
	else:
		velocity = Vector2(move_toward(velocity.x, 0, LOSS_SPEED), move_toward(velocity.y, 0, GAIN_SPEED));
	move_and_slide()
	
