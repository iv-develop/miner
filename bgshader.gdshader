shader_type canvas_item;

void vertex() {
}

void fragment() {
	vec4 tex = texture(TEXTURE, UV);
	float noise = tex.x;
	noise = round(noise * 8.0) / 8.0;
	if (noise > 0.4) {
		noise = noise + 0.1;
		noise = (1.0-tex.x) * noise;
	} else {
		noise *= 0.9;
	}
	COLOR = vec4(0.4, 0.15, 0.2, 1.0)
	 * vec4(noise, noise, noise, 1.0);
}

//void light() {
	// Called for every pixel for every light affecting the CanvasItem.
	// Uncomment to replace the default light processing function with this one.
//}
