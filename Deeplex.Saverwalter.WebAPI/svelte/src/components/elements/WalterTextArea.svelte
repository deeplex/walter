<script lang="ts">
    import {
        walter_subscribe_reset_changeTracker,
        walter_update_value
    } from '$walter/services/utils';
    import { TextArea, TextAreaSkeleton } from 'carbon-components-svelte';
    import { onMount } from 'svelte';

    export let value: string | undefined = undefined;
    export let labelText: string | undefined;
    export let readonly = false;

    let lastSavedValue: string | undefined;
    function updateLastSavedValue() {
        lastSavedValue = value || undefined;
    }

    onMount(() => {
        walter_subscribe_reset_changeTracker(updateLastSavedValue);
        updateLastSavedValue();
    });

    export let change = (e: Event) => {
        value = walter_update_value(
            lastSavedValue,
            value || undefined,
            '' + (e.target as any)?.value || undefined
        );
    };
</script>

{#await value}
    <TextAreaSkeleton />
{:then x}
    <TextArea {readonly} on:change={change} {labelText} value={x} />
{/await}
