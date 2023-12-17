<script lang="ts">
    import type { WalterS3FileWrapper } from '$walter/lib';
    import { ClickableTile } from 'carbon-components-svelte';

    export let href: string;
    export let s3ref: string;
    export let name: string;
    export let fileWrapper: WalterS3FileWrapper | undefined = undefined;

    if (fileWrapper) {
        fileWrapper.register(name, s3ref);
    }
</script>

<ClickableTile {href}>{name}</ClickableTile>
