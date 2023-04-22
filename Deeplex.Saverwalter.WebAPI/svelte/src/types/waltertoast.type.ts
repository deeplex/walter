export type WalterToastKind =
  | 'error'
  | 'info'
  | 'info-square'
  | 'success'
  | 'warning'
  | 'warning-alt'
  | undefined;

export type WalterToast = {
  title: string;
  subtitle: string;
  kind: WalterToastKind;
  timeout: false;
};
