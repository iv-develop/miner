shader_type canvas_item;

varying vec2 world_position;

uniform sampler2D noise_texture : source_color, filter_nearest, repeat_enable;
uniform vec2 noise_texture_size = vec2(512., 512.);

void vertex() {
	world_position = ((MODEL_MATRIX * vec4(VERTEX, 0., 1.)).xy - 0.5)/ 16.0;
}

void fragment() {
	vec2 pixpos = round(world_position * 16.0) / 16.0;
	float c = sin(pixpos.x + TIME) * cos(pixpos.y + TIME) * 0.5 + 1.0;
	float noise = texture(noise_texture, pixpos / 16.0).x;
	noise = round(noise * 8.0) / 8.0;
	if (noise > 0.4) {
		noise = noise + 0.1;
	} else {
		noise = 0.9;
	}
	noise *= 0.6;
	COLOR = texture(TEXTURE, UV) * vec4(noise, noise, noise, 1.0);
}

//void light() {
	// Called for every pixel for every light affecting the CanvasItem.
	// Uncomment to replace the default light processing function with this one.
//}
