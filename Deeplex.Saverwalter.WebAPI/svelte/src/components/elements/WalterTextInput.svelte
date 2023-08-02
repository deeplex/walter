<script lang="ts">
    import { TextInput, TextInputSkeleton } from 'carbon-components-svelte';

    export let value: string | undefined = undefined;
    export let labelText: string | undefined;
    export let readonly = false;
    export let required = false;

    export let change = (e: CustomEvent<string | number | null>) => {
        value = '' + e.detail || undefined;
    };
</script>

{#await value}
    <TextInputSkeleton />
{:then x}
    <TextInput
        invalid={required && !value}
        invalidText={`${labelText} ist ein notwendiges Feld.`}
        {readonly}
        on:change={change}
        {labelText}
        value={x} />
{/await}
