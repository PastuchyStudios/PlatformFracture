class PolyKRaycastHit
{
	var dist;
	var edge;
	var norm;
	var refl;

	function PolyKRaycastHit() {
		dist = 0;
		edge = 0;
		norm = new Vector2(0, 0);
		refl = new Vector2(0, 0);
	}
}
