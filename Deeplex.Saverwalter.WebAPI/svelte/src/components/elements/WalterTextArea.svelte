<script lang="ts">
    import { TextArea, TextAreaSkeleton } from 'carbon-components-svelte';

    export let value: string | undefined = undefined;
    export let labelText: string | undefined;
    export let readonly = false;

    export let change = (e: Event) => {
        value = '' + (e.target as any)?.value || undefined;
    };
</script>

{#await value}
    <TextAreaSkeleton />
{:then x}
    <TextArea {readonly} on:change={change} {labelText} value={x} />
{/await}
