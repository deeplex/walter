<script lang="ts">
    import type { WalterBetriebskostenabrechnungEntry } from "$walter/types";
    import {
        Row,
        StructuredList,
        StructuredListBody,
        StructuredListCell,
        StructuredListHead,
        StructuredListRow
    } from "carbon-components-svelte";

    export let abrechnung: WalterBetriebskostenabrechnungEntry;
</script>

<Row>
    <StructuredList style="margin-right: 2em">
        <StructuredListHead>
            <StructuredListRow head>
                <StructuredListCell head>Notizen</StructuredListCell>
            </StructuredListRow>
        </StructuredListHead>
        <StructuredListBody>
            {#each abrechnung.notes as note}
                <StructuredListRow>
                    <StructuredListCell>
                        {note.message}
                    </StructuredListCell>
                </StructuredListRow>
            {/each}
        </StructuredListBody>
    </StructuredList>
</Row>