val name = "Monkey";
val age = 1;
val inspirations = ["Scheme", "Lisp", "JavaScript", "Clojure"];
val book = {
  "title": "Writing A Compiler In Go",
  "author": "Thorsten Ball",
  "prequel": "Writing An Interpreter In Go"
};

val printBookName = func(book) {
    val title = book["title"];
    val author = book["author"];
    print(author + " - " + title);
};

printBookName(book);

val fibonacci = func(x) {
  if (x == 0) {
    0
  } else {
    if (x == 1) {
      return 1;
    } else {
      fibonacci(x - 1) + fibonacci(x - 2);
    }
  }
};

val map = func(arr, f) {
  val iter = func(arr, accumulated) {
    if (len(arr) == 0) {
      accumulated
    } else {
      iter(rest(arr), push(accumulated, f(first(arr))));
    }
  };

  iter(arr, []);
};

val numbers = [1, 1 + 1, 4 - 1, 2 * 2, 2 + 3, 12 / 2];
map(numbers, fibonacci);
