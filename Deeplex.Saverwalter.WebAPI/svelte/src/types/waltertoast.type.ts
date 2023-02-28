export type ToastKind = "error" | "info" | "info-square" | "success" | "warning" | "warning-alt" | undefined;

export type WalterToast = {
    title: string;
    subtitle: string;
    kind: ToastKind;
    timeout: false;
}