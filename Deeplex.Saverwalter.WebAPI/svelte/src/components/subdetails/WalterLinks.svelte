<script lang="ts">
    import { Accordion, Truncate } from 'carbon-components-svelte';
</script>

<Accordion size="xl" style="padding-bottom: 1em">
    <Truncate style="margin-bottom: 1em">Links :</Truncate>
    <slot />
</Accordion>
