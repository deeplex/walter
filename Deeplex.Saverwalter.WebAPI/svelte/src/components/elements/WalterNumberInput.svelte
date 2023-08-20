<script lang="ts">
    import { walter_subscribe_reset_changeTracker, walter_update_value } from '$walter/services/utils';
    import { NumberInput, NumberInputSkeleton } from 'carbon-components-svelte';
    import { onMount } from 'svelte';

    export let value: number | undefined = undefined;
    export let label: string | undefined;
    export let min: number | undefined = undefined;
    export let max: number | undefined = undefined;
    export let hideSteppers = true;
    export let digits = 2;
    export let readonly = false;
    export let required = false;


    let lastSavedValue: number | undefined;
    function updateLastSavedValue() {
        lastSavedValue = value;
    }

    onMount(() => {
        walter_subscribe_reset_changeTracker(updateLastSavedValue);
        updateLastSavedValue();
    });

    export let change = (e: CustomEvent<number | null>) => {
        if (e.detail === 0) {
            value = walter_update_value(lastSavedValue, value, 0);
        }
        else
        {
            value = walter_update_value(lastSavedValue, value, e.detail || undefined);
        }
    };

</script>

{#await value}
    <NumberInputSkeleton />
{:then x}
    <NumberInput
        invalid={required && value !== 0 && value === undefined}
        invalidText={`${label} ist ein notwendiges Feld.`}
        {readonly}
        {min}
        {max}
        {hideSteppers}
        on:change={change}
        {label}
        value={+(x || 0).toFixed(digits)}
    />
{/await}
