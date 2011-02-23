[expect ct-error]
[file]
<?php

error_reporting(0);

class pass {
	private static function show() {
		echo "Call show()\n";
	}

	public static function do_show() {
		pass::show();
	}
}

class fail extends pass {
	static function do_show() {
		pass::show();
	}
}

pass::do_show();
fail::do_show();

echo "Done\n"; // shouldn't be displayed
?>