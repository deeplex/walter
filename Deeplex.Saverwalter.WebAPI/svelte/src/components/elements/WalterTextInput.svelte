<script lang="ts">
    import { walter_subscribe_reset_changeTracker, walter_update_value } from '$walter/services/utils';
    import { TextInput, TextInputSkeleton } from 'carbon-components-svelte';
    import { onMount } from 'svelte';

    export let value: string | undefined = undefined;
    export let labelText: string | undefined;
    export let readonly = false;
    export let required = false;

    let lastSavedValue: string | undefined;
    function updateLastSavedValue() {
        lastSavedValue = value;
    }

    onMount(() => {
        walter_subscribe_reset_changeTracker(updateLastSavedValue);
        updateLastSavedValue();
    });

    export let change = (e: CustomEvent<string | number | null>) => {
        value = walter_update_value(    
            lastSavedValue,
            value,
            '' + e.detail || undefined);
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
