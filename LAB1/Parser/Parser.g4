grammar Parser;

options {
  language = CSharp;
}

expression 
  :  left=expression ('*' | '/' | 'div' | 'mod') right=expression   #MultiplicativeExpression
  |  op=('+' | '-') right=expression                                #UnaryExpression
  |  left=expression ('+' | '-') right=expression                   #AdditiveExpression
  |  'mmax' '(' exprList ')'                                        #MmaxFunction
  |  'mmin' '(' exprList ')'                                        #MminFunction
  |  '(' expression ')'                                             #ParenthesizedExpression
  |  NUMBER                                                         #Number
  |  CELL                                                           #Cell
  ;

exprList:  (expression (',' expression)*)?;

CELL: '&'[1-9][0-9]*'A'[1-9][0-9]*;

NUMBER: ('-')?[0-9]+'.'[0-9]+ | ('-')?[0-9]+;

WS: [ \t\r\n] -> skip;

INVALID: .;