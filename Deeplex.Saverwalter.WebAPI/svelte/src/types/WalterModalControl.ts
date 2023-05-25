export type WalterModalControl = {
    open: boolean;
    modalHeading: string;
    content: string;
    danger: boolean;
    primaryButtonText: string;
    submit: () => void;
};
