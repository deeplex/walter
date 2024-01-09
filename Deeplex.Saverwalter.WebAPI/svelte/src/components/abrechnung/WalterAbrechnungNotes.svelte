<!-- Copyright (C) 2023-2024  Kai Lawrence -->
<!--
This program is free software: you can redistribute it and/or modify
it under the terms of the GNU Affero General Public License as published
by the Free Software Foundation, either version 3 of the License, or
(at your option) any later version.

This program is distributed in the hope that it will be useful,
but WITHOUT ANY WARRANTY; without even the implied warranty of
MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
GNU Affero General Public License for more details.

You should have received a copy of the GNU Affero General Public License
along with this program. If not, see <https://www.gnu.org/licenses/>.
-->

<script lang="ts">
    import type { WalterBetriebskostenabrechnungEntry } from '$walter/types';
    import type { WalterBetriebskostenabrechnungNote } from '$walter/types/WalterBetriebskostenabrechnung.type';
    import { InlineNotification, Row, Tile } from 'carbon-components-svelte';

    export let abrechnung: WalterBetriebskostenabrechnungEntry;

    function getNotificationKind(note: WalterBetriebskostenabrechnungNote) {
        switch (`${note.severity}`) {
            case '0':
                return 'info';
            case '1':
                return 'warning';
            case '2':
                return 'error';
            default:
                return 'success';
        }
    }

    function getNotificationTitle(note: WalterBetriebskostenabrechnungNote) {
        switch (`${note.severity}`) {
            case '0':
                return 'Info';
            case '1':
                return 'Warnung';
            case '2':
                return 'Fehler';
            default:
                return 'Erfolg';
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
                    subtitle={note.message}
                />
            {/each}
        </Row>
    </div>
{/if}
