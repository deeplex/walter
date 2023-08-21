<script lang="ts">
    import type { WalterBetriebskostenabrechnungEntry } from "$walter/types";
    import type { WalterBetriebskostenabrechnungNote } from "$walter/types/WalterBetriebskostenabrechnung.type";
    import { InlineNotification, Row, Tile } from "carbon-components-svelte";

    export let abrechnung: WalterBetriebskostenabrechnungEntry;

    function getNotificationKind(note: WalterBetriebskostenabrechnungNote) {
        switch (`${note.severity}`) {
            case "0":
                return "info"
            case "1":
                return "warning";
            case "2":
                return "error";
            default:
                return "success";
        }
    }

    function getNotificationTitle(note: WalterBetriebskostenabrechnungNote) {
        switch (`${note.severity}`) {
            case "0":
                return "Info"
            case "1":
                return "Warnung";
            case "2":
                return "Fehler";
            default:
                return "Erfolg";
        }
    }

    let length = abrechnung.notes.length;
    function close() {
        length -= 1;
    }
</script>

{#if length > 0}
<div>
<Row style="margin-left: 0em">
    <Tile>
        <h4>Hinweise:</h4>
    </Tile>
</Row>
<Row>
    {#each abrechnung.notes as note}
        <InlineNotification
            style="margin-left: 2em; margin-top: 0.5em;"
            on:close={close}
            lowContrast
            kind={getNotificationKind(note)}
            title={getNotificationTitle(note)}
            subtitle={note.message} />
    {/each}
</Row>
</div>
{/if}