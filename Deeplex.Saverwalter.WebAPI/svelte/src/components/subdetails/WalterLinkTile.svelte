<script lang="ts">
    import type { WalterFileWrapper } from '$walter/lib';
    import { ClickableTile } from 'carbon-components-svelte';

    export let href: string;
    export let fileref: string;
    export let name: string;
    export let fileWrapper: WalterFileWrapper | undefined = undefined;

    if (fileWrapper) {
        fileWrapper.register(name, fileref);
    }
</script>

<ClickableTile {href}>{name}</ClickableTile>
