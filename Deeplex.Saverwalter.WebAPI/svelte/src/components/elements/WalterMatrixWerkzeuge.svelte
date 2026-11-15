<!-- Copyright (C) 2023-2026  Kai Lawrence -->
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

<!--
    Suche und „Nur Offene“ für die Matrizen. Der Zustand gehört der Seite, damit er
    beim Wechsel zwischen den Tabs und in der URL erhalten bleibt.
-->
<script lang="ts">
    import { Search, Toggle } from 'carbon-components-svelte';

    export let suche = '';
    export let nurOffene = false;
</script>

<div class="werkzeuge">
    <div class="suche">
        <Search
            size="sm"
            bind:value={suche}
            placeholder="Suche mit ; separierter Liste..."
        />
    </div>
    <div class="schalter">
        <span class="schalter-text">Nur Offene</span>
        <Toggle
            size="sm"
            hideLabel
            labelText="Nur Offene"
            labelA="Aus"
            labelB="An"
            bind:toggled={nurOffene}
        />
    </div>
</div>

<style>
    .werkzeuge {
        display: flex;
        gap: 1.5rem;
        align-items: center;
        flex-wrap: wrap;
        margin: 1rem 0 0.75rem;
    }

    .suche {
        flex: 1 1 18rem;
        max-width: 32rem;
    }

    .schalter {
        display: flex;
        flex: none;
        gap: 0.75rem;
        align-items: center;
        white-space: nowrap;
    }

    .schalter-text {
        font-size: 0.875rem;
    }
</style>
