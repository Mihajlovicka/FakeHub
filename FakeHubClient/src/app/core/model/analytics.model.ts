type LogCondition = {
  field: 'level' | 'message' | 'timestamp';
  operator: 'AND' | 'OR' | 'NOT';
  value: string;
  openParen?: boolean;
  closeParen?: boolean;
};