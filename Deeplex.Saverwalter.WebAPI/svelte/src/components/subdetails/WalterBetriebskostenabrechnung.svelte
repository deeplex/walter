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
    import { WalterNumberInput } from '$walter/components';
    import { Button, Row } from 'carbon-components-svelte';
    import { post_files } from './WalterBetriebskostenabrechnung';

    export let fetchImpl: typeof fetch;
    export let id: number;
    let jahr: number = new Date().getFullYear() - 1;

    function click() {
        post_files(id, jahr, fetchImpl);
    }
</script>

<Row>
    <WalterNumberInput bind:value={jahr} label="Jahr" hideSteppers={false} />
    <Button on:click={click}>Betriebskostenabrechnung erstellen</Button>
</Row>
