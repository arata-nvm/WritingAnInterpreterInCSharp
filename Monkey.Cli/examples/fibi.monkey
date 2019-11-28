val fib = func(n) {
  if n < 3 {
    return 1
  }
  var a = 1
  var b = 1
  var c = 0
  var i = 0
  while i < n - 2 {
    c = a + b
    b = a
    a = c
    i = i + 1
  }
  return a
}

print(fib(35))